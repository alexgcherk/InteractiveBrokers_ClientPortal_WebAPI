// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Clients;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class MarketDataIntegrationTests : IntegrationTestBase
{
    // EUR/USD on IDEALPRO — well-known stable conid
    private const long EurUsdConid = 12087792;

    [Test]
    public async Task GetSnapshot_EurUsd_ReturnsLastPrice()
    {
        var fields = string.Join(",",
            MarketDataFields.Last, MarketDataFields.Bid, MarketDataFields.Ask,
            MarketDataFields.BidSize, MarketDataFields.AskSize);

        var result = await Client.MarketData.GetSnapshotAsync(
            EurUsdConid.ToString(), fields);

        result.Should().NotBeNullOrEmpty();
        TestContext.WriteLine("EUR/USD snapshot:");
        foreach (var snap in result!)
            TestContext.WriteLine($"  conid={snap.Conid}, last={snap.Last}, bid={snap.Bid}, ask={snap.Ask}");
    }

    [Test]
    public async Task GetSnapshot_MultipleConids_ReturnsMultipleEntries()
    {
        // EUR/USD and Apple (265598)
        var conids = $"{EurUsdConid},265598";
        var result = await Client.MarketData.GetSnapshotAsync(
            conids, MarketDataFields.Last);

        result.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"Multi-conid snapshot count: {result!.Length}");
    }

    [Test]
    public async Task GetHistory_EurUsd_5Days_1Hour_ReturnsBars()
    {
        var result = await Client.MarketData.GetHistoryAsync(
            EurUsdConid, "5d", "1h");

        result.Should().NotBeNull();
        result!.Symbol.Should().NotBeNullOrEmpty();
        result.Data.Should().NotBeNullOrEmpty("5 days of hourly bars should exist");
        TestContext.WriteLine($"Symbol: {result.Symbol}, bars: {result.Data!.Length}");
        TestContext.WriteLine($"Latest bar: open={result.Data.Last().Open}, close={result.Data.Last().Close}");
    }

    [Test]
    public async Task GetHistory_EurUsd_1Month_Daily_ReturnsBars()
    {
        var result = await Client.MarketData.GetHistoryAsync(
            EurUsdConid, "1m", "1d");

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"1-month daily bars: {result.Data!.Length}");
    }

    [Test]
    public async Task GetHistory_Aapl_5Days_1Hour_ReturnsBars()
    {
        var result = await Client.MarketData.GetHistoryAsync(
            265598, "5d", "1h");

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"AAPL bars: {result.Data!.Length}, symbol={result.Symbol}");
    }

    [Test]
    public async Task UnsubscribeAll_ReturnsTrue()
    {
        var result = await Client.MarketData.UnsubscribeAllAsync();

        result.Should().NotBeNull();
        result!.Unsubscribed.Should().BeTrue();
        TestContext.WriteLine("All subscriptions cancelled");
    }

    [Test]
    public async Task GetAltSnapshot_AfterSubscription_ReturnsData()
    {
        // Subscribe via primary endpoint first
        await Client.MarketData.GetSnapshotAsync(
            EurUsdConid.ToString(), MarketDataFields.Last);
        await WaitAsync();

        // Alt endpoint
        var result = await Client.MarketData.GetAltSnapshotAsync(
            EurUsdConid.ToString(), MarketDataFields.Last);

        result.Should().NotBeNull();
        TestContext.WriteLine($"Alt snapshot entries: {result!.Length}");
    }
}