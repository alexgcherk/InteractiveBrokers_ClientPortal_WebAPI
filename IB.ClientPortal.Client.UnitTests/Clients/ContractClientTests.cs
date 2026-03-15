// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class ContractClientTests
{
    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetInfoAsync_ReturnsParsedContractInfo()
    {
        const string json = """
                            {
                              "con_id": 265598,
                              "symbol": "AAPL",
                              "sec_type": "STK",
                              "exchange": "NASDAQ",
                              "currency": "USD",
                              "companyName": "Apple Inc."
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Contracts.GetInfoAsync(265598);

        result!.ConId.Should().Be(265598);
        result.Symbol.Should().Be("AAPL");
        result.Exchange.Should().Be("NASDAQ");
        result.CompanyName.Should().Be("Apple Inc.");
    }

    [Test]
    public async Task SearchAsync_ReturnsParsedSecDefResults()
    {
        const string json = """
                            [
                              {
                                "conid": 265598,
                                "companyHeader": "AAPL - APPLE INC",
                                "companyName": "APPLE INC",
                                "symbol": "AAPL",
                                "description": "APPLE INC",
                                "sections": [
                                  {"secType": "STK", "exchange": "NASDAQ"},
                                  {"secType": "OPT", "months": "JAN26;FEB26;MAR26"}
                                ]
                              }
                            ]
                            """;

        using var client = CreateClient(json);
        var result = await client.Contracts.SearchAsync("AAPL");

        result.Should().HaveCount(1);
        result![0].Conid.Should().Be(265598);
        result![0].Symbol.Should().Be("AAPL");
        result![0].Sections.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_WithSecType_AppendsQueryParam()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("[]");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Contracts.SearchAsync("AAPL", "OPT");

        var query = getCapture()!.RequestUri!.Query;
        query.Should().Contain("symbol=AAPL");
        query.Should().Contain("secType=OPT");
    }

    [Test]
    public async Task GetStrikesAsync_ReturnsParsedStrikes()
    {
        const string json = """
                            {
                              "call": [175.0, 180.0, 185.0, 190.0],
                              "put":  [175.0, 180.0, 185.0, 190.0]
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Contracts.GetStrikesAsync(265598, "JAN26");

        result!.Call.Should().HaveCount(4);
        result.Put.Should().Contain(180.0);
    }

    [Test]
    public async Task GetFuturesAsync_ReturnsParsedFutures()
    {
        const string json = """
                            {
                              "ES": [
                                {"symbol": "ESM6", "conid": 600001, "underConid": 11004968, "expiry": "20260620", "shortName": "ES Jun26"},
                                {"symbol": "ESU6", "conid": 600002, "underConid": 11004968, "expiry": "20260919", "shortName": "ES Sep26"}
                              ]
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Contracts.GetFuturesAsync("ES");

        result.Should().ContainKey("ES");
        result!["ES"].Should().HaveCount(2);
        result["ES"][0].Symbol.Should().Be("ESM6");
    }

    [Test]
    public async Task GetRulesAsync_SendsPostWithBody()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Contracts.GetRulesAsync(new ContractRulesRequest { Conid = 265598, IsBuy = true });

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Post);
        var body = getCapturedBody()!;
        body.Should().Contain("265598");
        body.Should().Contain("true");
    }
}