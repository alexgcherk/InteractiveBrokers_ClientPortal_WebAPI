// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client;
using IB.ClientPortal.Client.Models;
using IBClientPortal.Client;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests;

/// <summary>
///     Base class providing shared helpers for all integration test fixtures.
///     All fixtures inherit this class — no setup/teardown here so derived classes
///     can define their own <c>[OneTimeSetUp]</c> and <c>[SetUp]</c> freely.
/// </summary>
public abstract class IntegrationTestBase
{
    protected static IBPortalClient Client => GlobalSetup.Client;
    protected static string AccountId => GlobalSetup.Settings.AccountId;

    // ── Rate limit helpers ────────────────────────────────────────────────────

    /// <summary>Waits 6 seconds to respect the 1-req/5s limit on order endpoints.</summary>
    protected static Task WaitOrderRateLimitAsync()
    {
        return Task.Delay(TimeSpan.FromSeconds(6));
    }

    /// <summary>Waits 200 ms between normal endpoint calls (global 10 req/s limit).</summary>
    protected static Task WaitAsync()
    {
        return Task.Delay(200);
    }

    // ── Order placement helper ────────────────────────────────────────────────

    /// <summary>
    ///     Places an order and transparently handles the IBKR warning-reply flow.
    ///     Returns the <c>order_id</c> string on success, or throws if the order was rejected.
    /// </summary>
    protected static async Task<string> PlaceAndConfirmOrderAsync(
        string accountId, PlaceOrderBody order)
    {
        var responses = await Client.Orders.PlaceOrdersAsync(accountId, [order]);

        if (responses is null || responses.Length == 0)
            throw new InvalidOperationException("PlaceOrdersAsync returned no response");

        var response = responses[0];

        // Handle the reply flow (IBKR may require confirming one or more warnings)
        var maxReplies = 5;
        while (response.ReplyId is not null && response.OrderId is null && maxReplies-- > 0)
        {
            TestContext.Progress.WriteLine(
                $"  ↳ Order reply required ({response.ReplyId}): " +
                string.Join("; ", response.Message ?? []));

            await Task.Delay(600); // brief pause before reply
            var replyResponses = await Client.Orders.ReplyAsync(response.ReplyId);

            if (replyResponses is null || replyResponses.Length == 0)
                throw new InvalidOperationException("ReplyAsync returned no response");

            response = replyResponses[0];
        }

        if (response.OrderId is null)
        {
            var msgs = string.Join(", ", response.Message ?? []);
            throw new InvalidOperationException(
                $"Order was not placed — no order_id returned. Messages: {msgs}");
        }

        return response.OrderId;
    }

    /// <summary>Cancels an order, suppressing exceptions (used in teardown).</summary>
    protected static async Task CancelOrderSafeAsync(string? orderId)
    {
        if (orderId is null) return;
        try
        {
            await Client.Orders.CancelOrderAsync(AccountId, long.Parse(orderId));
            await WaitOrderRateLimitAsync();
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"  [Cleanup] Could not cancel order {orderId}: {ex.Message}");
        }
    }

    /// <summary>Searches for an FX pair contract and falls back to the known conid.</summary>
    protected static async Task<long> FindFxConidAsync(string symbol, long fallback)
    {
        try
        {
            var results = await Client.Contracts.SearchAsync(symbol, "CASH");
            // CASH FX pairs: root-level secType="CASH", symbol like "EUR.USD", sections is EMPTY
            var match = results?.FirstOrDefault(r =>
                r.SecType == "CASH" &&
                r.Symbol?.StartsWith(symbol, StringComparison.OrdinalIgnoreCase) == true);

            if (match?.Conid > 0)
            {
                TestContext.Progress.WriteLine($"  Found {symbol} FX conid via search: {match.Conid} ({match.Symbol})");
                return match.Conid;
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"  Search failed for {symbol}: {ex.Message}");
        }

        TestContext.Progress.WriteLine($"  Using hardcoded conid for {symbol}: {fallback}");
        return fallback;
    }
}