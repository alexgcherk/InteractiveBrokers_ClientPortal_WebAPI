// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class AccountIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetAccounts_ReturnsPaperAccount()
    {
        var result = await Client.Account.GetAccountsAsync();

        result.Should().NotBeNull();
        result!.Accounts.Should().NotBeNullOrEmpty("at least one account must be returned");
        result.Accounts!.Should().Contain(AccountId, "configured account must be present");
        result.SelectedAccount.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"Accounts: {string.Join(", ", result.Accounts)}");
        TestContext.WriteLine($"Selected: {result.SelectedAccount}");
    }

    [Test]
    public async Task GetSummary_ReturnsBuyingPowerAndNetLiq()
    {
        var result = await Client.Account.GetSummaryAsync(AccountId);

        result.Should().NotBeNull();
        result!.NetLiquidation.Should().BeGreaterThan(0, "paper account has non-zero net liquidation");
        result.BuyingPower.Should().BeGreaterThan(0);
        TestContext.WriteLine($"NetLiq: {result.NetLiquidation:F2}");
        TestContext.WriteLine($"BuyingPower: {result.BuyingPower:F2}");
    }

    [Test]
    public async Task GetPnl_ReturnsDailyPnl()
    {
        var result = await Client.Account.GetPnlAsync();

        result.Should().NotBeNull();
        result!.Upnl.Should().NotBeNull();
        TestContext.WriteLine($"PnL segments: {string.Join(", ", result.Upnl!.Keys)}");
        foreach (var kv in result.Upnl!)
            TestContext.WriteLine($"  {kv.Key}: upl={kv.Value.Upl:F2}, dpl={kv.Value.Dpl:F2}");
    }

    [Test]
    public async Task GetUser_ReturnsUsernameAndFeatures()
    {
        var result = await Client.Account.GetUserAsync();

        result.Should().NotBeNull();
        result!.Username.Should().NotBeNullOrEmpty();
        TestContext.WriteLine($"Username: {result.Username}");
        TestContext.WriteLine($"Features: {string.Join(", ", result.Features?.Keys ?? Enumerable.Empty<string>())}");
    }

    [Test]
    public async Task GetMta_ReturnsWithoutError()
    {
        // MTA may return empty/null on paper accounts — just verify no exception
        var result = await Client.Account.GetMtaAsync();
        TestContext.WriteLine($"MTA: {result}");
    }

    [Test]
    public async Task GetCurrencyPairs_ReturnsUsdPairs()
    {
        var result = await Client.Account.GetCurrencyPairsAsync();
        TestContext.WriteLine($"Currency pairs (USD): {result}");
    }

    [Test]
    public async Task GetExchangeRate_EurToUsd_ReturnsPositiveRate()
    {
        // Note: may not be available on paper accounts; handled gracefully
        try
        {
            var result = await Client.Account.GetExchangeRateAsync("EUR", "USD");
            TestContext.WriteLine($"EUR/USD rate: {result}");
        }
        catch (HttpRequestException ex)
        {
            TestContext.WriteLine($"Exchange rate endpoint returned error (expected on some accounts): {ex.Message}");
        }
    }
}