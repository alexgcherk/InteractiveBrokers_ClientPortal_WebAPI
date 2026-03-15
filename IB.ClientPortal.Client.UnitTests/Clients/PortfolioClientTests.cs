// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class PortfolioClientTests
{
    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetAccountsAsync_ReturnsParsedPortfolioAccounts()
    {
        const string json = """
                            [
                              {
                                "id": "DU0000000",
                                "accountId": "DU0000000",
                                "accountVan": "DU0000000",
                                "accountTitle": "Paper Account",
                                "currency": "USD",
                                "type": "DEMO",
                                "tradingType": "STKCASH"
                              }
                            ]
                            """;

        using var client = CreateClient(json);
        var result = await client.Portfolio.GetAccountsAsync();

        result.Should().HaveCount(1);
        result![0].AccountId.Should().Be("DU0000000");
        result![0].Currency.Should().Be("USD");
    }

    [Test]
    public async Task GetPositionsAsync_ReturnsEmptyList_WhenNoPositions()
    {
        using var client = CreateClient("[]");
        var result = await client.Portfolio.GetPositionsAsync("DU0000000");
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetPositionsAsync_ReturnsParsedPositions()
    {
        const string json = """
                            [
                              {
                                "acctId": "DU0000000",
                                "conid": 265598,
                                "contractDesc": "AAPL",
                                "position": 100,
                                "mktPrice": 182.50,
                                "mktValue": 18250.0,
                                "avgCost": 175.00,
                                "unrealizedPnl": 750.0,
                                "currency": "USD",
                                "ticker": "AAPL",
                                "secType": "STK"
                              }
                            ]
                            """;

        using var client = CreateClient(json);
        var result = await client.Portfolio.GetPositionsAsync("DU0000000");

        result.Should().HaveCount(1);
        result![0].Conid.Should().Be(265598);
        result![0].Quantity.Should().Be(100);
        result![0].MarketPrice.Should().Be(182.50);
        result![0].UnrealizedPnl.Should().Be(750.0);
    }

    [Test]
    public async Task GetAllocationAsync_ReturnsParsedAllocation()
    {
        const string json = """
                            {
                              "assetClass": {
                                "long": {"STK": 0.85, "CASH": 0.15},
                                "short": {}
                              }
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Portfolio.GetAllocationAsync("DU0000000");

        result!.AssetClass!.Long.Should().ContainKey("STK");
        result.AssetClass.Long!["STK"].Should().Be(0.85);
    }

    [Test]
    public async Task GetLedgerAsync_ReturnsParsedCurrencyLedgers()
    {
        const string json = """
                            {
                              "USD": {
                                "cashbalance": 95000.00,
                                "netliquidationvalue": 100000.0,
                                "currency": "USD",
                                "exchangerate": 1.0
                              }
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Portfolio.GetLedgerAsync("DU0000000");

        result.Should().ContainKey("USD");
        result!["USD"].CashBalance.Should().Be(95000.0);
        result["USD"].Currency.Should().Be("USD");
    }

    [Test]
    public async Task GetPositionsAsync_SendsCorrectPath()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("[]");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Portfolio.GetPositionsAsync("DU0000000", 2);

        getCapture()!.RequestUri!.PathAndQuery.Should().Contain("DU0000000/positions/2");
    }
}