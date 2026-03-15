// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Clients;
using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class MarketDataClientTests
{
    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetSnapshotAsync_ReturnsParsedSnapshot()
    {
        const string json = """
                            [
                              {
                                "conid": 265598,
                                "31": "182.50",
                                "84": "182.45",
                                "86": "182.55",
                                "87": "42500000",
                                "_updated": 1700000000000
                              }
                            ]
                            """;

        using var client = CreateClient(json);
        var result = await client.MarketData.GetSnapshotAsync("265598", "31,84,86,87");

        result.Should().HaveCount(1);
        result![0].Conid.Should().Be(265598);
        result![0].Last.Should().Be("182.50");
        result![0].Bid.Should().Be("182.45");
        result![0].Ask.Should().Be("182.55");
    }

    [Test]
    public async Task GetSnapshotAsync_SendsCorrectQueryParams()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("[]");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.MarketData.GetSnapshotAsync("265598,8314", "31,84,86");

        var query = getCapture()!.RequestUri!.Query;
        query.Should().Contain("conids=265598");
        query.Should().Contain("fields=31");
    }

    [Test]
    public async Task GetHistoryAsync_ReturnsParsedBars()
    {
        const string json = """
                            {
                              "serverId": "srv-1",
                              "symbol": "AAPL",
                              "priceFactor": 1.0,
                              "data": [
                                {"t": 1700000000000, "o": 180.0, "h": 183.0, "l": 179.5, "c": 182.5, "v": 50000}
                              ],
                              "points": 1
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.MarketData.GetHistoryAsync(265598, "5d", "1h");

        result!.Symbol.Should().Be("AAPL");
        result.Data.Should().HaveCount(1);
        result.Data![0].Open.Should().Be(180.0);
        result.Data![0].High.Should().Be(183.0);
        result.Data![0].Close.Should().Be(182.5);
    }

    [Test]
    public async Task GetHistoryAsync_SendsCorrectQueryParams()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("""{"data":[]}""");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.MarketData.GetHistoryAsync(265598, "1m", "1d", "NASDAQ");

        var query = getCapture()!.RequestUri!.Query;
        query.Should().Contain("conid=265598");
        query.Should().Contain("period=1m");
        query.Should().Contain("bar=1d");
        query.Should().Contain("exchange=NASDAQ");
    }

    [Test]
    public async Task UnsubscribeAllAsync_ReturnsTrue()
    {
        const string json = """{"unsubscribed": true}""";
        using var client = CreateClient(json);
        var result = await client.MarketData.UnsubscribeAllAsync();
        result!.Unsubscribed.Should().BeTrue();
    }

    [Test]
    public async Task RunScannerAsync_SendsCorrectBody()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("""{"contracts":[]}""");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        var request = new ScannerRunRequest
        {
            Instrument = "STK",
            Type = "TOP_PERC_GAIN",
            Size = "25"
        };
        await client.MarketData.RunScannerAsync(request);

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Post);
        req.RequestUri!.PathAndQuery.Should().Contain("scanner/run");
        var body = getCapturedBody()!;
        body.Should().Contain("TOP_PERC_GAIN");
    }

    [Test]
    public void MarketDataFields_EquityDefault_ContainsExpectedFields()
    {
        MarketDataFields.EquityDefault.Should().Contain(MarketDataFields.Last);
        MarketDataFields.EquityDefault.Should().Contain(MarketDataFields.Bid);
        MarketDataFields.EquityDefault.Should().Contain(MarketDataFields.Ask);
        MarketDataFields.EquityDefault.Should().Contain(MarketDataFields.Volume);
    }
}