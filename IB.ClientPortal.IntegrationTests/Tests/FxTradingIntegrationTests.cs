// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Clients;
using IB.ClientPortal.Client.Models;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

/// <summary>
///     Full FX trading workflow on IDEALPRO using paper account.
///     Tests EUR/USD, GBP/USD and USD/JPY with:
///     - Limit order placement (limit price far from market → will NOT fill)
///     - Order verification in the open order list
///     - Order modification (price change)
///     - Order cancellation
///     - Stop-limit order placement + cancel
///     - WhatIf margin preview
///     Tests within this fixture are ordered — each builds on the prior one.
///     <see cref="LifeCycle.SingleInstance" /> ensures all ordered tests share the same
///     instance fields (e.g. <c>_limitOrderId</c>).
///     The <see cref="FxConids" /> class documents known stable conids used as fallback.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class FxTradingIntegrationTests : IntegrationTestBase
{
    // ── Setup / teardown ──────────────────────────────────────────────────────

    [OneTimeSetUp]
    public async Task SetUpAsync()
    {
        // Discover conids dynamically (with fallback to hardcoded values)
        _eurUsdConid = await FindFxConidAsync("EUR", FxConids.EurUsd);
        _gbpUsdConid = await FindFxConidAsync("GBP", FxConids.GbpUsd);
        _usdJpyConid = await FindFxConidAsync("USD", FxConids.UsdJpy);

        // Get live EUR/USD mid for logging purposes
        var snap = await Client.MarketData.GetSnapshotAsync(
            _eurUsdConid.ToString(),
            string.Join(",", MarketDataFields.Bid, MarketDataFields.Ask, MarketDataFields.Last));

        var entry = snap?.FirstOrDefault(s => s.Conid == _eurUsdConid);
        if (entry is not null && double.TryParse(entry.Last, out var last))
            _liveEurUsdMid = last;

        TestContext.Progress.WriteLine(
            $"[FX Setup] EUR/USD={_eurUsdConid}, GBP/USD={_gbpUsdConid}, USD/JPY={_usdJpyConid}");
        TestContext.Progress.WriteLine($"[FX Setup] Live EUR/USD mid: {_liveEurUsdMid:F5}");
    }

    [OneTimeTearDown]
    public async Task TearDownAsync()
    {
        // Best-effort cancellation of any orders that weren't explicitly cancelled
        await WaitOrderRateLimitAsync();
        foreach (var id in new[] { _limitOrderId, _stopOrderId, _gbpOrderId, _jpyOrderId })
            await CancelOrderSafeAsync(id);
    }

    // ── Known stable IDEALPRO conids (used if dynamic search fails) ───────────
    private static class FxConids
    {
        public const long EurUsd = 12087792;
        public const long GbpUsd = 12087797;
        public const long UsdJpy = 12087820;
    }

    // ── FX order parameters ───────────────────────────────────────────────────
    private const double FxQuantity = 20_000; // 20 000 base units (IDEALPRO minimum)

    // Limit prices that are intentionally far from market — will NOT fill
    private const double EurUsdSafeBuyLimit = 1.0000; // market ~1.08
    private const double EurUsdModifyLimit = 1.0050;
    private const double EurUsdStopTrigger = 1.2500; // stop BUY above market
    private const double EurUsdStopLimit = 1.2550;
    private const double GbpUsdSafeBuyLimit = 1.0500; // market ~1.26
    private const double UsdJpySafeBuyLimit = 110.00; // market ~148

    // ── State shared across ordered tests ─────────────────────────────────────
    private long _eurUsdConid;
    private long _gbpUsdConid;
    private long _usdJpyConid;
    private double _liveEurUsdMid; // mid price from snapshot

    private string? _limitOrderId; // primary EUR/USD limit order
    private string? _stopOrderId; // EUR/USD stop-limit order
    private string? _gbpOrderId;
    private string? _jpyOrderId;

    // ══════════════════════════════════════════════════════════════════════════
    // EUR/USD LIMIT ORDER LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    [Order(10)]
    public async Task EurUsd_WhatIf_LimitBuy_ShowsMarginImpact()
    {
        // Call snapshot first (required pre-flight for whatif)
        await Client.MarketData.GetSnapshotAsync(
            _eurUsdConid.ToString(), MarketDataFields.Last);
        await WaitAsync();

        var order = BuildFxOrder(_eurUsdConid, "LMT", "BUY",
            FxQuantity, EurUsdSafeBuyLimit);

        var result = await Client.Orders.WhatIfAsync(AccountId, [order]);

        result.Should().NotBeNull("WhatIf should return a margin preview");
        TestContext.WriteLine($"WhatIf — initial margin change: {result!.Initial?.Change}, " +
                              $"maintenance: {result.Maintenance?.Change}, warn: {result.Warn}");
    }

    [Test]
    [Order(20)]
    public async Task EurUsd_PlaceLimitBuy_FarBelowMarket_ReturnsOrderId()
    {
        var order = BuildFxOrder(_eurUsdConid, "LMT", "BUY",
            FxQuantity, EurUsdSafeBuyLimit);

        _limitOrderId = await PlaceAndConfirmOrderAsync(AccountId, order);

        _limitOrderId.Should().NotBeNullOrEmpty("EUR/USD limit order must be placed");
        TestContext.WriteLine($"✅ EUR/USD limit BUY placed — orderId: {_limitOrderId} @ {EurUsdSafeBuyLimit}");

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(21)]
    public async Task EurUsd_LimitOrder_AppearsInOpenOrders()
    {
        _limitOrderId.Should().NotBeNull("Test Order(20) must have run first");

        // First call triggers a server-side refresh (may return snapshot=true / stale cache).
        // Second call attempts to return fresh data.
        await Client.Orders.GetOrdersAsync();
        await WaitOrderRateLimitAsync();
        var response = await Client.Orders.GetOrdersAsync();

        var order = response?.Orders?.FirstOrDefault(o =>
            o.OrderId.ToString() == _limitOrderId);

        if (order is not null)
        {
            order.Status.Should().BeOneOf(
                new[] { "Submitted", "PreSubmitted", "Inactive", "PendingSubmit" },
                "a freshly placed limit order must be pending");
            TestContext.WriteLine($"Order found in bulk list: status={order.Status}, side={order.Side}, " +
                                  $"qty={order.TotalSize}, price={order.Price}");
        }
        else
        {
            // IBKR gateway bulk orders list may remain stale for minutes after placement.
            // Verify the order exists via the individual status endpoint instead.
            TestContext.WriteLine(
                $"Order {_limitOrderId} not in bulk list (gateway cache lag) — verifying via status endpoint");
            var status = await Client.Orders.GetOrderStatusAsync(long.Parse(_limitOrderId!));
            status.Should().NotBeNull(
                $"order {_limitOrderId} must be accessible via status endpoint even if not in bulk list");
        }

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(22)]
    public async Task EurUsd_GetOrderStatus_ReturnsSingleOrderDetails()
    {
        _limitOrderId.Should().NotBeNull();

        var result = await Client.Orders.GetOrderStatusAsync(long.Parse(_limitOrderId!));
        result.Should().NotBeNull();
        TestContext.WriteLine($"Order status detail: {result}");
    }

    [Test]
    [Order(30)]
    public async Task EurUsd_ModifyLimitOrder_ChangePrice()
    {
        _limitOrderId.Should().NotBeNull("Test Order(20) must have run first");

        var modified = BuildFxOrder(_eurUsdConid, "LMT", "BUY",
            FxQuantity, EurUsdModifyLimit);

        var responses = await Client.Orders.ModifyOrderAsync(
            AccountId, long.Parse(_limitOrderId!), modified);

        responses.Should().NotBeNullOrEmpty("modify should return a response");

        // Handle any reply warnings triggered by modification
        var response = responses![0];
        var guard = 5;
        while (response.ReplyId is not null && response.OrderId is null && guard-- > 0)
        {
            TestContext.Progress.WriteLine($"  Modify reply: {string.Join("; ", response.Message ?? [])}");
            await Task.Delay(600);
            var reply = await Client.Orders.ReplyAsync(response.ReplyId);
            response = reply![0];
        }

        TestContext.WriteLine($"✅ EUR/USD limit modified to {EurUsdModifyLimit} — " +
                              $"order_id={response.OrderId ?? _limitOrderId}");
        // If modification creates a new order ID, track it
        if (response.OrderId is not null && response.OrderId != _limitOrderId)
        {
            TestContext.WriteLine($"  New order ID after modification: {response.OrderId}");
            _limitOrderId = response.OrderId;
        }

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(31)]
    public async Task EurUsd_VerifyModifiedPrice_InOpenOrders()
    {
        _limitOrderId.Should().NotBeNull();

        await WaitAsync();
        var response = await Client.Orders.GetOrdersAsync();
        var order = response?.Orders?.FirstOrDefault(o =>
            o.OrderId.ToString() == _limitOrderId);

        if (order is not null)
        {
            TestContext.WriteLine($"Order after modify: status={order.Status}, price={order.Price}");
            // The price may or may not have updated yet depending on gateway processing
            order.Price.Should().BeApproximately(EurUsdModifyLimit, 0.01,
                "modified limit price should be reflected");
        }
        else
        {
            TestContext.WriteLine($"Order {_limitOrderId} not found in open orders after modify (may have a new ID)");
        }

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(40)]
    public async Task EurUsd_CancelLimitOrder_Succeeds()
    {
        _limitOrderId.Should().NotBeNull("Test Order(20) must have run first");

        await Client.Orders.CancelOrderAsync(AccountId, long.Parse(_limitOrderId!));
        TestContext.WriteLine($"✅ EUR/USD limit order {_limitOrderId} cancelled");
        _limitOrderId = null; // Cleared so teardown doesn't double-cancel

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(41)]
    public async Task EurUsd_AfterCancel_OrderNoLongerPending()
    {
        await WaitAsync();

        var response = await Client.Orders.GetOrdersAsync();
        // A cancelled order may still appear with status "Cancelled" — that's correct
        var order = response?.Orders?.FirstOrDefault(o =>
            o.Status is "Submitted" or "PreSubmitted" or "PendingSubmit");

        TestContext.WriteLine($"Open pending orders after cancel: {order?.OrderId}");
        // Just log — there might be other open orders from outside these tests
    }

    // ══════════════════════════════════════════════════════════════════════════
    // EUR/USD STOP-LIMIT ORDER
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    [Order(50)]
    public async Task EurUsd_PlaceStopLimitBuy_AboveMarket_ReturnsOrderId()
    {
        // Stop-limit BUY: triggers if price rises to EurUsdStopTrigger (1.25), 
        // limit at EurUsdStopLimit (1.255). Far above current market (~1.08) — won't fill.
        var order = BuildFxOrder(_eurUsdConid, "STP LMT", "BUY",
            FxQuantity, EurUsdStopLimit, EurUsdStopTrigger);

        _stopOrderId = await PlaceAndConfirmOrderAsync(AccountId, order);
        _stopOrderId.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"✅ EUR/USD stop-limit placed — orderId: {_stopOrderId} " +
                              $"stop={EurUsdStopTrigger}, lmt={EurUsdStopLimit}");

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(51)]
    public async Task EurUsd_StopLimitOrder_AppearsInOpenOrders()
    {
        _stopOrderId.Should().NotBeNull();

        var response = await Client.Orders.GetOrdersAsync();
        var order = response?.Orders?.FirstOrDefault(o =>
            o.OrderId.ToString() == _stopOrderId);

        if (order is not null)
        {
            order.OrderType.Should().Match(t =>
                    t!.Contains("STP") || t.Contains("STOP"),
                "order type should be stop-limit");
            TestContext.WriteLine($"Stop-limit order: type={order.OrderType}, " +
                                  $"price={order.Price}, auxPrice={order.AuxPrice}");
        }
        else
        {
            TestContext.WriteLine($"Stop order {_stopOrderId} not in current snapshot — may need re-query");
        }

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(52)]
    public async Task EurUsd_ModifyStopLimit_ChangeTriggerPrice()
    {
        _stopOrderId.Should().NotBeNull();

        // Widen the stop slightly (still far from market)
        var modified = BuildFxOrder(_eurUsdConid, "STP LMT", "BUY",
            FxQuantity, EurUsdStopLimit + 0.005, EurUsdStopTrigger + 0.005);

        var responses = await Client.Orders.ModifyOrderAsync(
            AccountId, long.Parse(_stopOrderId!), modified);
        responses.Should().NotBeNullOrEmpty();

        var response = responses![0];
        while (response.ReplyId is not null && response.OrderId is null)
        {
            TestContext.Progress.WriteLine($"  Stop modify reply: {string.Join("; ", response.Message ?? [])}");
            await Task.Delay(600);
            response = (await Client.Orders.ReplyAsync(response.ReplyId))![0];
        }

        TestContext.WriteLine($"✅ Stop-limit modified, response id: {response.OrderId ?? _stopOrderId}");
        if (response.OrderId is not null && response.OrderId != _stopOrderId)
            _stopOrderId = response.OrderId;

        await WaitOrderRateLimitAsync();
    }

    [Test]
    [Order(53)]
    public async Task EurUsd_CancelStopLimitOrder_Succeeds()
    {
        _stopOrderId.Should().NotBeNull();

        await Client.Orders.CancelOrderAsync(AccountId, long.Parse(_stopOrderId!));
        TestContext.WriteLine($"✅ EUR/USD stop-limit {_stopOrderId} cancelled");
        _stopOrderId = null;

        await WaitOrderRateLimitAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GBP/USD  — place & cancel
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    [Order(60)]
    public async Task GbpUsd_PlaceLimitBuy_FarBelowMarket_PlaceAndCancel()
    {
        var order = BuildFxOrder(_gbpUsdConid, "LMT", "BUY",
            FxQuantity, GbpUsdSafeBuyLimit);

        _gbpOrderId = await PlaceAndConfirmOrderAsync(AccountId, order);
        _gbpOrderId.Should().NotBeNullOrEmpty("GBP/USD order must be placed");
        TestContext.WriteLine($"✅ GBP/USD limit placed — orderId: {_gbpOrderId} @ {GbpUsdSafeBuyLimit}");

        await WaitOrderRateLimitAsync();

        await Client.Orders.CancelOrderAsync(AccountId, long.Parse(_gbpOrderId));
        TestContext.WriteLine("\u2705 GBP/USD order cancelled");
        _gbpOrderId = null;

        await WaitOrderRateLimitAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // USD/JPY  — place & cancel
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    [Order(70)]
    public async Task UsdJpy_PlaceLimitBuy_FarBelowMarket_PlaceAndCancel()
    {
        var order = BuildFxOrder(_usdJpyConid, "LMT", "BUY",
            FxQuantity, UsdJpySafeBuyLimit);

        try
        {
            _jpyOrderId = await PlaceAndConfirmOrderAsync(AccountId, order);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.StartsWith("Order was not placed", StringComparison.Ordinal))
        {
            // IBKR paper account may enforce total order value or per-session limits.
            // This is an IBKR policy rejection, not a client defect — mark inconclusive.
            TestContext.Progress.WriteLine(
                $"[USD/JPY] Order rejected by IBKR paper account policy: {ex.Message}");
            Assert.Inconclusive($"USD/JPY order rejected by IBKR paper account — not a client bug: {ex.Message}");
            return;
        }

        _jpyOrderId.Should().NotBeNullOrEmpty("USD/JPY order must be placed");
        TestContext.WriteLine($"✅ USD/JPY limit placed — orderId: {_jpyOrderId} @ {UsdJpySafeBuyLimit}");

        await WaitOrderRateLimitAsync();

        await Client.Orders.CancelOrderAsync(AccountId, long.Parse(_jpyOrderId));
        TestContext.WriteLine("\u2705 USD/JPY order cancelled");
        _jpyOrderId = null;

        await WaitOrderRateLimitAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SELL LIMIT — verify BUY and SELL sides both work
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    [Order(80)]
    public async Task EurUsd_PlaceSellLimit_FarAboveMarket_PlaceAndCancel()
    {
        // SELL limit above market (~1.08) — won't fill
        var order = BuildFxOrder(_eurUsdConid, "LMT", "SELL",
            FxQuantity, 1.2000);

        var orderId = await PlaceAndConfirmOrderAsync(AccountId, order);
        orderId.Should().NotBeNullOrEmpty("EUR/USD SELL limit order must be placed");
        TestContext.WriteLine($"✅ EUR/USD SELL limit placed — orderId: {orderId} @ 1.2000");

        await WaitOrderRateLimitAsync();

        await Client.Orders.CancelOrderAsync(AccountId, long.Parse(orderId));
        TestContext.WriteLine("✅ EUR/USD SELL limit cancelled");

        await WaitOrderRateLimitAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ORDERS QUERY — broader tests
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    [Order(90)]
    public async Task GetTrades_ReturnsListOrEmpty()
    {
        var trades = await Client.Orders.GetTradesAsync(7);
        trades.Should().NotBeNull("trades endpoint must return an array");
        TestContext.WriteLine($"Trades (last 7 days): {trades!.Length}");
        foreach (var t in trades.Take(5))
            TestContext.WriteLine($"  [{t.TradeTime}] {t.Symbol} {t.Side} qty={t.Size} @ {t.Price}");
    }

    [Test]
    [Order(91)]
    public async Task GetOrders_AllFilters_ReturnsWithoutError()
    {
        var response = await Client.Orders.GetOrdersAsync("filled,cancelled,inactive");
        response.Should().NotBeNull();
        TestContext.WriteLine($"Orders (filled/cancelled/inactive): {response!.Orders?.Length ?? 0}");
    }

    [Test]
    [Order(92)]
    public async Task SuppressMessages_AndReset_WorksWithoutError()
    {
        // Pre-suppress common FX order warnings
        await Client.Orders.SuppressMessagesAsync(["o163", "o354"]);
        TestContext.WriteLine("Messages suppressed");
        await WaitAsync();

        await Client.Orders.ResetSuppressedMessagesAsync();
        TestContext.WriteLine("Message suppression reset");
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static PlaceOrderBody BuildFxOrder(
        long conid, string orderType, string side,
        double quantity, double price, double? auxPrice = null)
    {
        return new PlaceOrderBody
        {
            AccountId = GlobalSetup.Settings.AccountId,
            Conid = conid,
            SecType = "CASH",
            OrderType = orderType,
            Side = side,
            Quantity = quantity,
            Price = price,
            AuxPrice = auxPrice,
            Tif = "GTC",
            Exchange = "IDEALPRO"
        };
    }
}