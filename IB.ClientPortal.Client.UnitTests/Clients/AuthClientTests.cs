// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

namespace IB.ClientPortal.Client.UnitTests.Clients;

[TestFixture]
public class AuthClientTests
{
    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    private IBPortalClient _sut = null!;

    private IBPortalClient CreateClient(string responseJson)
    {
        var handler = MockHttpHandler.For(responseJson);
        return new IBPortalClient(MockHttpHandler.DefaultOptions(), handler.Object);
    }

    [Test]
    public async Task GetStatusAsync_WhenAuthenticated_ReturnsAuthenticatedStatus()
    {
        // Arrange
        const string json =
            """{"authenticated":true,"established":true,"competing":false,"connected":true,"MAC":"00:11:22"}""";
        _sut = CreateClient(json);

        // Act
        var result = await _sut.Auth.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Authenticated.Should().BeTrue();
        result.Established.Should().BeTrue();
        result.Competing.Should().BeFalse();
        result.MAC.Should().Be("00:11:22");
    }

    [Test]
    public async Task GetStatusAsync_WhenNotAuthenticated_ReturnsUnauthenticatedStatus()
    {
        const string json = """{"authenticated":false,"established":false,"competing":false,"connected":false}""";
        _sut = CreateClient(json);

        var result = await _sut.Auth.GetStatusAsync();

        result!.Authenticated.Should().BeFalse();
        result.Established.Should().BeFalse();
    }

    [Test]
    public async Task TickleAsync_ReturnsParsedResponse()
    {
        const string json = """{"session":"abc123","ssoExpires":300,"collission":false,"userId":12345}""";
        _sut = CreateClient(json);

        var result = await _sut.Auth.TickleAsync();

        result.Should().NotBeNull();
        result!.Session.Should().Be("abc123");
        result.SsoExpires.Should().Be(300);
        result.UserId.Should().Be(12345);
    }

    [Test]
    public async Task ReauthenticateAsync_ReturnsTriggeredMessage()
    {
        const string json = """{"message":"triggered"}""";
        _sut = CreateClient(json);

        var result = await _sut.Auth.ReauthenticateAsync();

        result!.Message.Should().Be("triggered");
    }

    [Test]
    public async Task InitSsoDhAsync_ReturnsParsedAuthStatus()
    {
        const string json = """{"authenticated":true,"established":true,"competing":false,"connected":true}""";
        _sut = CreateClient(json);

        var result = await _sut.Auth.InitSsoDhAsync();

        result!.Authenticated.Should().BeTrue();
    }
}