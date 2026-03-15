// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class OrderClientTests
{
    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetOrdersAsync_NoFilters_ReturnsParsedOrders()
    {
        const string json = """
                            {
                              "orders": [
                                {
                                  "orderId": 123456,
                                  "acct": "DU0000000",
                                  "conid": 265598,
                                  "ticker": "AAPL",
                                  "secType": "STK",
                                  "orderType": "LMT",
                                  "side": "BUY",
                                  "totalSize": 10,
                                  "price": 180.00,
                                  "status": "Submitted",
                                  "tif": "DAY"
                                }
                              ],
                              "snapshot": true
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Orders.GetOrdersAsync();

        result!.Orders.Should().HaveCount(1);
        result.Orders![0].OrderId.Should().Be(123456);
        result.Orders![0].Ticker.Should().Be("AAPL");
        result.Orders![0].Side.Should().Be("BUY");
        result.Snapshot.Should().BeTrue();
    }

    [Test]
    public async Task GetOrdersAsync_WithFilters_AppendsQueryParam()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("""{"orders":[],"snapshot":true}""");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Orders.GetOrdersAsync("filled,cancelled");

        var req = getCapture();
        req!.RequestUri!.Query.Should().Contain("filters=filled");
    }

    [Test]
    public async Task GetTradesAsync_ReturnsParsedTrades()
    {
        const string json = """
                            [
                              {
                                "execution_id": "exec-001",
                                "symbol": "AAPL",
                                "side": "B",
                                "size": 10,
                                "price": "179.50",
                                "account": "DU0000000",
                                "conid": 265598,
                                "sec_type": "STK"
                              }
                            ]
                            """;

        using var client = CreateClient(json);
        var result = await client.Orders.GetTradesAsync();

        result.Should().HaveCount(1);
        result![0].Symbol.Should().Be("AAPL");
        result![0].ExecutionId.Should().Be("exec-001");
    }

    [Test]
    public async Task PlaceOrdersAsync_SendsCorrectPayload()
    {
        var (mock, getCapture, getCapturedBody) =
            MockHttpHandler.CapturingSetup("""[{"order_id":"ord-1","order_status":"Submitted"}]""");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        var order = new PlaceOrderBody
        {
            AccountId = "DU0000000",
            Conid = 265598,
            OrderType = "LMT",
            Side = "BUY",
            Quantity = 5,
            Price = 180.00,
            Tif = "DAY"
        };
        var result = await client.Orders.PlaceOrdersAsync("DU0000000", [order]);

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Post);
        req.RequestUri!.PathAndQuery.Should().Contain("DU0000000/orders");

        var body = getCapturedBody()!;
        body.Should().Contain("265598");
        body.Should().Contain("LMT");

        result.Should().HaveCount(1);
        result![0].OrderId.Should().Be("ord-1");
    }

    [Test]
    public async Task PlaceOrdersAsync_WhenWarningReturned_ExposesReplyId()
    {
        const string json = """[{"id":"reply-abc","message":["Warning: some message"]}]""";
        using var client = CreateClient(json);

        var order = new PlaceOrderBody { Conid = 265598, OrderType = "MKT", Side = "BUY", Quantity = 1 };
        var result = await client.Orders.PlaceOrdersAsync("DU0000000", [order]);

        result![0].ReplyId.Should().Be("reply-abc");
        result[0].Message.Should().Contain("Warning: some message");
    }

    [Test]
    public async Task ReplyAsync_SendsConfirmedTrue()
    {
        var (mock, getCapture, getCapturedBody) =
            MockHttpHandler.CapturingSetup("""[{"order_id":"ord-2","order_status":"Submitted"}]""");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Orders.ReplyAsync("reply-abc", true);

        var req = getCapture();
        req!.RequestUri!.PathAndQuery.Should().Contain("reply/reply-abc");
        var body = getCapturedBody()!;
        body.Should().Contain("\"confirmed\":true");
    }

    [Test]
    public async Task CancelOrderAsync_CallsDeleteWithCorrectPath()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Orders.CancelOrderAsync("DU0000000", 999001);

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Delete);
        req.RequestUri!.PathAndQuery.Should().Contain("DU0000000/order/999001");
    }

    [Test]
    public async Task SuppressMessagesAsync_SendsMessageIds()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Orders.SuppressMessagesAsync(["o163", "o354"]);

        var body = getCapturedBody()!;
        body.Should().Contain("o163");
        body.Should().Contain("o354");
    }
}