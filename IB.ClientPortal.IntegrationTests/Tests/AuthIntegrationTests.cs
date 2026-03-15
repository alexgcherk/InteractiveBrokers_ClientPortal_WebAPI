// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class AuthIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetStatus_ReturnsAuthenticatedStatus()
    {
        var status = await Client.Auth.GetStatusAsync();

        status.Should().NotBeNull();
        status!.Authenticated.Should().BeTrue("gateway session must be authenticated");
        status.Established.Should().BeTrue("brokerage session must be established");
        TestContext.WriteLine($"MAC: {status.MAC}, Competing: {status.Competing}");
    }

    [Test]
    public async Task Tickle_ReturnsSessionInfo()
    {
        var result = await Client.Auth.TickleAsync();

        result.Should().NotBeNull();
        result!.Session.Should().NotBeNullOrEmpty("tickle should return session ID");
        result.UserId.Should().BeGreaterThan(0);
        TestContext.WriteLine($"Session: {result.Session}, SSO expires in: {result.SsoExpires}s");
    }

    [Test]
    public async Task InitSsoDh_ReturnsStatus()
    {
        var result = await Client.Auth.InitSsoDhAsync();

        result.Should().NotBeNull();
        // May return authenticated or trigger a new session — either is valid
        TestContext.WriteLine($"SsoDh init: authenticated={result!.Authenticated}, established={result.Established}");
    }

    [Test]
    public async Task Reauthenticate_ReturnsTriggeredMessage()
    {
        var result = await Client.Auth.ReauthenticateAsync();

        result.Should().NotBeNull();
        result!.Message.Should().Be("triggered");
    }

    [Test]
    public async Task GetStatus_AfterTickle_StillAuthenticated()
    {
        await Client.Auth.TickleAsync();
        await WaitAsync();

        var status = await Client.Auth.GetStatusAsync();

        status!.Authenticated.Should().BeTrue();
    }
}