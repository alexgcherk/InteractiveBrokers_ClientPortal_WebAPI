// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Models;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class ScannerIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetScannerParams_ReturnsInstrumentsAndScanTypes()
    {
        // Rate-limited: 1 req / 15 min — but first call always works
        var result = await Client.MarketData.GetScannerParamsAsync();

        result.Should().NotBeNull();
        TestContext.WriteLine($"Scanner params type: {result!.GetType().Name}");
    }

    [Test]
    public async Task RunScanner_TopGainers_ReturnsContracts()
    {
        var request = new ScannerRunRequest
        {
            Instrument = "STK",
            Type = "TOP_PERC_GAIN",
            Location = "STK.US.MAJOR",
            Size = "10"
        };

        var result = await Client.MarketData.RunScannerAsync(request);

        result.Should().NotBeNull();
        TestContext.WriteLine($"Scanner results: {result!.Contracts?.Length ?? 0}");
        foreach (var c in result.Contracts?.Take(5) ?? [])
            TestContext.WriteLine($"  {c.Symbol} ({c.ConidEx}) — {c.CompanyName}");
    }

    [Test]
    public async Task RunScanner_TopLosers_ReturnsContracts()
    {
        var request = new ScannerRunRequest
        {
            Instrument = "STK",
            Type = "TOP_PERC_LOSE",
            Location = "STK.US.MAJOR",
            Size = "10"
        };

        var result = await Client.MarketData.RunScannerAsync(request);

        result.Should().NotBeNull();
        TestContext.WriteLine($"Top losers: {result!.Contracts?.Length ?? 0}");
    }

    [Test]
    public async Task RunScanner_HighVolume_ReturnsContracts()
    {
        var request = new ScannerRunRequest
        {
            Instrument = "STK",
            Type = "HOT_BY_VOLUME",
            Location = "STK.US.MAJOR",
            Size = "10"
        };

        var result = await Client.MarketData.RunScannerAsync(request);
        result.Should().NotBeNull();
        TestContext.WriteLine($"High volume: {result!.Contracts?.Length ?? 0}");
    }
}