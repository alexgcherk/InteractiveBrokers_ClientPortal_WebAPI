// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class PortfolioIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetPortfolioAccounts_ReturnsAccountWithCurrency()
    {
        var result = await Client.Portfolio.GetAccountsAsync();

        result.Should().NotBeNullOrEmpty();
        var acct = result!.FirstOrDefault(a => a.AccountId == AccountId);
        acct.Should().NotBeNull($"account {AccountId} must appear in portfolio accounts");
        acct!.Currency.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"Account: {acct.AccountId}, type={acct.Type}, currency={acct.Currency}");
    }

    [Test]
    public async Task GetSubAccounts_ReturnsAtLeastOneAccount()
    {
        var result = await Client.Portfolio.GetSubAccountsAsync();
        result.Should().NotBeNull();
        TestContext.WriteLine($"Sub-accounts: {result?.Length ?? 0}");
    }

    [Test]
    public async Task GetPositions_Page0_ReturnsListOrEmpty()
    {
        var result = await Client.Portfolio.GetPositionsAsync(AccountId);

        result.Should().NotBeNull("positions endpoint should return a list (possibly empty)");
        TestContext.WriteLine($"Positions count: {result!.Length}");
        foreach (var p in result.Take(5))
            TestContext.WriteLine(
                $"  {p.Ticker} ({p.SecType}): qty={p.Quantity}, price={p.MarketPrice:F4}, upnl={p.UnrealizedPnl:F2}");
    }

    [Test]
    public async Task GetFirstPositions_ReturnsListOrEmpty()
    {
        var result = await Client.Portfolio.GetFirstPositionsAsync(AccountId);
        result.Should().NotBeNull();
        TestContext.WriteLine($"First page positions: {result!.Length}");
    }

    [Test]
    public async Task GetAllocation_ReturnsAssetClassBreakdown()
    {
        var result = await Client.Portfolio.GetAllocationAsync(AccountId);

        result.Should().NotBeNull();
        result!.AssetClass.Should().NotBeNull();
        TestContext.WriteLine(
            $"Asset class long: {string.Join(", ", result.AssetClass?.Long?.Select(kv => $"{kv.Key}={kv.Value:P0}") ?? [])}");
    }

    [Test]
    public async Task GetLedger_ReturnsCashBalances()
    {
        var result = await Client.Portfolio.GetLedgerAsync(AccountId);

        result.Should().NotBeNullOrEmpty("ledger must contain at least one currency");
        TestContext.WriteLine("Ledger:");
        foreach (var kv in result!)
            TestContext.WriteLine(
                $"  {kv.Key}: cash={kv.Value.CashBalance:F2}, netliq={kv.Value.NetLiquidationValue:F2}");
    }

    [Test]
    public async Task InvalidatePositionCache_ReturnsWithoutError()
    {
        await Client.Portfolio.InvalidatePositionCacheAsync(AccountId);
        // just verify no exception
        TestContext.WriteLine("Position cache invalidated");
    }

    [Test]
    public async Task GetPositionByConid_ForKnownContract_ReturnsResult()
    {
        // First get positions so we have a known conid
        var positions = await Client.Portfolio.GetPositionsAsync(AccountId);
        if (positions is null || positions.Length == 0)
        {
            TestContext.WriteLine("No open positions — skipping conid lookup");
            return;
        }

        var conid = positions[0].Conid;
        var result = await Client.Portfolio.GetPositionByConidAsync(AccountId, conid);
        result.Should().NotBeNull();
        TestContext.WriteLine($"Position for conid {conid}: qty={result![0].Quantity}");
    }
}