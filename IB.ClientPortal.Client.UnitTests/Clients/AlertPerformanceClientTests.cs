// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class AlertClientTests
{
    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetAlertsAsync_ReturnsEmptyList_WhenNoAlerts()
    {
        using var client = CreateClient("[]");
        var result = await client.Alerts.GetAlertsAsync("DU0000000");
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAlertsAsync_ReturnsParsedAlerts()
    {
        const string json = """
                            [
                              {
                                "order_id": 9001,
                                "account": "DU0000000",
                                "alert_name": "AAPL above 200",
                                "alert_active": 1,
                                "order_time": "20250101-10:00:00"
                              }
                            ]
                            """;

        using var client = CreateClient(json);
        var result = await client.Alerts.GetAlertsAsync("DU0000000");

        result.Should().HaveCount(1);
        result![0].OrderId.Should().Be(9001);
        result![0].AlertName.Should().Be("AAPL above 200");
        result![0].AlertActive.Should().Be(1);
    }

    [Test]
    public async Task CreateAlertAsync_SendsPostToCorrectPath()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        var alert = new CreateAlertRequest
        {
            AlertName = "AAPL Price Alert",
            AlertActive = 1
        };
        await client.Alerts.CreateAlertAsync("DU0000000", alert);

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Post);
        req.RequestUri!.PathAndQuery.Should().Contain("DU0000000/alert");
        var body = getCapturedBody()!;
        body.Should().Contain("AAPL Price Alert");
    }

    [Test]
    public async Task SetAlertActiveAsync_SendsActivateWithCorrectBody()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Alerts.SetAlertActiveAsync("DU0000000", 9001L, false);

        var body = getCapturedBody()!;
        body.Should().Contain("9001");
        body.Should().Contain("\"alertActive\":0");
    }

    [Test]
    public async Task DeleteAlertAsync_SendsDeleteToCorrectPath()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Alerts.DeleteAlertAsync("DU0000000", 9001L);

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Delete);
        req.RequestUri!.PathAndQuery.Should().Contain("DU0000000/alert/9001");
    }
}

[TestFixture]
public class PerformanceClientTests
{
    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetPerformanceAsync_SendsPostWithAccountIds()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Performance.GetPerformanceAsync(["DU0000000"], "D");

        var req = getCapture();
        req!.Method.Should().Be(HttpMethod.Post);
        req.RequestUri!.PathAndQuery.Should().Contain("pa/performance");
        var body = getCapturedBody()!;
        body.Should().Contain("DU0000000");
        body.Should().Contain("\"period\":\"D\"");
    }

    [Test]
    public async Task GetTransactionsAsync_SendsPostWithAllFields()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Performance.GetTransactionsAsync(["DU0000000"], [265598, 272093], "EUR");

        var body = getCapturedBody()!;
        body.Should().Contain("EUR");
        body.Should().Contain("265598");
    }
}