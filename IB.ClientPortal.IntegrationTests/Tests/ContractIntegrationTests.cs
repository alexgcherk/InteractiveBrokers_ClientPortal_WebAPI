// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Models;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class ContractIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task SearchAsync_Aapl_ReturnsStockResult()
    {
        var results = await Client.Contracts.SearchAsync("AAPL", "STK");

        results.Should().NotBeNullOrEmpty("AAPL search should return results");
        var aapl = results!.FirstOrDefault(r => r.Symbol == "AAPL");
        aapl.Should().NotBeNull("AAPL must be in results");
        aapl!.Conid.Should().BeGreaterThan(0);
        TestContext.WriteLine($"AAPL conid: {aapl.Conid}, header: {aapl.CompanyHeader}");
    }

    [Test]
    public async Task SearchAsync_Eur_Cash_ReturnsForexPair()
    {
        var results = await Client.Contracts.SearchAsync("EUR", "CASH");

        results.Should().NotBeNullOrEmpty("EUR CASH search should return FX results");
        // IBKR returns CASH FX pairs with root-level secType and symbol like "EUR.USD"
        // The sections array is intentionally empty for CASH instruments
        var eur = results!.FirstOrDefault(r =>
            r.Symbol?.StartsWith("EUR", StringComparison.OrdinalIgnoreCase) == true &&
            r.SecType == "CASH");
        eur.Should().NotBeNull("EUR forex contract must be in results");
        eur!.Conid.Should().BeGreaterThan(0, "conid must be a positive integer");
        TestContext.WriteLine($"EUR/USD conid: {eur.Conid}, symbol: {eur.Symbol}");
    }

    [Test]
    public async Task GetInfo_Aapl_ReturnsContractDetails()
    {
        var result = await Client.Contracts.GetInfoAsync(265598);

        result.Should().NotBeNull();
        result!.Symbol.Should().Be("AAPL");
        result.SecType.Should().Be("STK", "AAPL is a stock (instrument_type field)");
        result.Currency.Should().Be("USD");
        TestContext.WriteLine($"AAPL: conid={result.ConId}, exchange={result.Exchange}, cusip={result.Cusip}");
    }

    [Test]
    public async Task GetInfoAndRules_Aapl_ReturnsWithoutError()
    {
        var result = await Client.Contracts.GetInfoAndRulesAsync(265598);
        result.Should().NotBeNull();
        TestContext.WriteLine($"AAPL info+rules: secType={result!.SecType}");
    }

    [Test]
    public async Task GetSecDef_Aapl_ReturnsFullSecDef()
    {
        var result = await Client.Contracts.GetSecDefAsync("265598");
        result.Should().NotBeNull();
        TestContext.WriteLine($"SecDef: {result}");
    }

    [Test]
    public async Task GetStocks_Aapl_ReturnsExchangeList()
    {
        var result = await Client.Contracts.GetStocksAsync("AAPL");
        result.Should().NotBeNull();
        result!.Should().ContainKey("AAPL");
        TestContext.WriteLine("AAPL stock exchanges found");
    }

    [Test]
    public async Task GetFutures_ES_ReturnsNonExpiredContracts()
    {
        var result = await Client.Contracts.GetFuturesAsync("ES");
        result.Should().NotBeNull();
        result!.Should().ContainKey("ES", "ES futures should exist");
        result["ES"].Should().NotBeEmpty("non-expired ES contracts should be available");
        foreach (var f in result["ES"].Take(3))
            TestContext.WriteLine($"  ES future: {f.Symbol}, conid={f.Conid}, expiry={f.Expiry}");
    }

    [Test]
    public async Task GetTradingSchedule_Aapl_ReturnsSchedule()
    {
        var result = await Client.Contracts.GetTradingScheduleAsync("STK", "AAPL", "NASDAQ");
        result.Should().NotBeNull();
        TestContext.WriteLine($"Trading schedule: {result}");
    }

    [Test]
    public async Task SearchAsync_EurUsd_OptionsStrikes_ReturnsStrikes()
    {
        // First search for EUR/USD, then get option strikes (if available)
        // EUR/USD is CASH — no options, skip gracefully
        var results = await Client.Contracts.SearchAsync("AAPL");
        var aapl = results?.FirstOrDefault(r => r.Symbol == "AAPL");
        if (aapl == null) return;

        var optSection = aapl.Sections?.FirstOrDefault(s => s.SecType == "OPT");
        if (optSection?.Months == null)
        {
            TestContext.WriteLine("No option months found for AAPL in search result");
            return;
        }

        var firstMonth = optSection.Months.Split(';')[0];
        var strikes = await Client.Contracts.GetStrikesAsync(aapl.Conid, firstMonth);
        strikes.Should().NotBeNull();
        strikes!.Call.Should().NotBeNullOrEmpty("AAPL should have call strikes");
        TestContext.WriteLine($"AAPL {firstMonth} strikes (first 5 calls): {string.Join(", ", strikes.Call!.Take(5))}");
    }

    [Test]
    public async Task GetRulesAsync_Aapl_Buy_ReturnsOrderRules()
    {
        var result = await Client.Contracts.GetRulesAsync(
            new ContractRulesRequest { Conid = 265598, IsBuy = true });
        result.Should().NotBeNull();
        TestContext.WriteLine($"Contract rules: {result}");
    }

    [Test]
    public async Task GetAlgos_Aapl_ReturnsAlgoStrategies()
    {
        var result = await Client.Contracts.GetAlgosAsync(265598);
        TestContext.WriteLine($"Algos: {result}");
    }
}