// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using FluentAssertions;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class AccountClientTests
{
    private IBPortalClient CreateClient(string responseJson, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = MockHttpHandler.For(responseJson, status);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetAccountsAsync_ReturnsParsedAccounts()
    {
        const string json = """
                            {
                              "accounts": ["DU0000000"],
                              "aliases": {"DU0000000": "Paper Account"},
                              "selectedAccount": "DU0000000",
                              "sessionId": "sess-abc"
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Account.GetAccountsAsync();

        result.Should().NotBeNull();
        result!.Accounts.Should().ContainSingle("DU0000000");
        result.SelectedAccount.Should().Be("DU0000000");
        result.SessionId.Should().Be("sess-abc");
        result.Aliases.Should().ContainKey("DU0000000");
    }

    [Test]
    public async Task GetSummaryAsync_ReturnsParsedAccountSummary()
    {
        const string json = """
                            {
                              "accountType": "INDIVIDUAL",
                              "netLiquidationValue": 100000.0,
                              "buyingPower": 50000.0
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Account.GetSummaryAsync("DU0000000");

        result!.AccountType.Should().Be("INDIVIDUAL");
        result.NetLiquidation.Should().Be(100000.0);
        result.BuyingPower.Should().Be(50000.0);
    }

    [Test]
    public async Task GetPnlAsync_ReturnsParsedPnl()
    {
        const string json = """
                            {
                              "upnl": {
                                "DU0000000.Core": {"rowType": 1, "dpl": -10.5, "nl": 99999.0, "upl": 200.0, "el": 0, "mv": 5000.0}
                              }
                            }
                            """;

        using var client = CreateClient(json);
        var result = await client.Account.GetPnlAsync();

        result!.Upnl.Should().ContainKey("DU0000000.Core");
        result.Upnl!["DU0000000.Core"].Upl.Should().Be(200.0);
    }

    [Test]
    public async Task GetUserAsync_ReturnsParsedUser()
    {
        const string json = """{"username": "testuser", "features": {"env": "PROD"}}""";

        using var client = CreateClient(json);
        var result = await client.Account.GetUserAsync();

        result!.Username.Should().Be("testuser");
    }

    [Test]
    public async Task SwitchAccountAsync_SendsCorrectBody()
    {
        var (mock, getCapture, getCapturedBody) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Account.SwitchAccountAsync("DU9999999");

        var req = getCapture();
        req.Should().NotBeNull();
        req!.Method.Should().Be(HttpMethod.Post);
        var body = getCapturedBody()!;
        body.Should().Contain("DU9999999");
    }
}