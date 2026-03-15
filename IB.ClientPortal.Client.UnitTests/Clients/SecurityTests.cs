// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace IB.ClientPortal.Client.UnitTests;

[TestFixture]
public class SecurityTests
{
    // ── IBPortalClientOptions defaults ───────────────────────────────────────

    [Test]
    public void DefaultOptions_IgnoreSslErrors_IsFalse()
    {
        new IBPortalClientOptions().IgnoreSslErrors.Should().BeFalse();
    }

    [Test]
    public void DefaultOptions_BaseUrl_PointsToLocalhost()
    {
        new IBPortalClientOptions().BaseUrl.Should().StartWith("https://localhost");
    }

    // ── SSL bypass: localhost variants are always bypassed ───────────────────

    [TestCase("https://localhost:5000")]
    [TestCase("https://localhost")]
    [TestCase("https://LOCALHOST:5000")]
    [TestCase("https://127.0.0.1:5000")]
    [TestCase("https://127.0.0.2:5000")]
    [TestCase("https://[::1]:5000")]
    public void ShouldBypassSslValidation_LoopbackHost_ReturnsTrue(string baseUrl)
    {
        var options = new IBPortalClientOptions { BaseUrl = baseUrl, IgnoreSslErrors = false };

        IBPortalHttpClient.ShouldBypassSslValidation(options).Should().BeTrue();
    }

    // ── SSL bypass: remote hosts are NOT bypassed unless IgnoreSslErrors=true ─

    [TestCase("https://trading:5000")]
    [TestCase("https://192.168.1.100:5000")]
    [TestCase("https://mybroker.example.com")]
    public void ShouldBypassSslValidation_RemoteHost_AndIgnoreSslErrorsFalse_ReturnsFalse(string baseUrl)
    {
        var options = new IBPortalClientOptions { BaseUrl = baseUrl, IgnoreSslErrors = false };

        IBPortalHttpClient.ShouldBypassSslValidation(options).Should().BeFalse();
    }

    // ── SSL bypass: explicit IgnoreSslErrors=true overrides for any host ─────

    [TestCase("https://trading:5000")]
    [TestCase("https://192.168.1.100:5000")]
    [TestCase("https://mybroker.example.com")]
    public void ShouldBypassSslValidation_RemoteHost_AndIgnoreSslErrorsTrue_ReturnsTrue(string baseUrl)
    {
        var options = new IBPortalClientOptions { BaseUrl = baseUrl, IgnoreSslErrors = true };

        IBPortalHttpClient.ShouldBypassSslValidation(options).Should().BeTrue();
    }

    // ── PingAsync exception handling ─────────────────────────────────────────

    [Test]
    public async Task PingAsync_WhenHttpRequestExceptionThrown_ReturnsFalse()
    {
        var mock = ThrowingHandler(new HttpRequestException("Connection refused"));
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        var result = await client.Auth.PingAsync();

        result.Should().BeFalse();
    }

    [Test]
    public async Task PingAsync_WhenTaskCanceledExceptionThrown_ReturnsFalse()
    {
        var mock = ThrowingHandler(new TaskCanceledException("Timed out"));
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        var result = await client.Auth.PingAsync();

        result.Should().BeFalse();
    }

    [Test]
    public async Task PingAsync_WhenGatewayRespondsFalse_ReturnsFalse()
    {
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(),
            MockHttpHandler.For("false").Object);

        var result = await client.Auth.PingAsync();

        result.Should().BeFalse();
    }

    [Test]
    public async Task PingAsync_WhenGatewayRespondsTrue_ReturnsTrue()
    {
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(),
            MockHttpHandler.For("true").Object);

        var result = await client.Auth.PingAsync();

        result.Should().BeTrue();
    }

    [Test]
    public void PingAsync_WhenUnexpectedExceptionThrown_PropagatesException()
    {
        // Non-network exceptions (e.g. programming errors) must not be silently swallowed.
        var mock = ThrowingHandler(new InvalidOperationException("unexpected"));
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        Func<Task> act = () => client.Auth.PingAsync();

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── URL encoding: special characters in query string parameters ──────────

    [Test]
    public async Task GetHistoryAsync_WhenPeriodContainsSpecialChars_EncodesQueryParam()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.MarketData.GetHistoryAsync(265598, "1d&inject=x", "1min");

        getCapture()!.RequestUri!.Query.Should().Contain("period=1d%26inject%3Dx");
    }

    [Test]
    public async Task GetHistoryAsync_WhenBarContainsSpecialChars_EncodesQueryParam()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.MarketData.GetHistoryAsync(265598, "1d", "1min&x=1");

        getCapture()!.RequestUri!.Query.Should().Contain("bar=1min%26x%3D1");
    }

    [Test]
    public async Task GetHistoryAsync_WhenExchangeContainsSpecialChars_EncodesQueryParam()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.MarketData.GetHistoryAsync(265598, "1d", "1min", exchange: "NA S&Q");

        getCapture()!.RequestUri!.Query.Should().Contain("exchange=NA%20S%26Q");
    }

    [Test]
    public async Task GetCurrencyPairsAsync_WhenCurrencyContainsSpecialChars_EncodesQueryParam()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Account.GetCurrencyPairsAsync("US&D");

        getCapture()!.RequestUri!.Query.Should().Contain("currency=US%26D");
    }

    [Test]
    public async Task GetExchangeRateAsync_WhenSourceOrTargetContainsSpecialChars_EncodesQueryParams()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Account.GetExchangeRateAsync("U S", "A&B");

        var query = getCapture()!.RequestUri!.Query;
        query.Should().Contain("source=U%20S");
        query.Should().Contain("target=A%26B");
    }

    [Test]
    public async Task GetBondFiltersAsync_WhenParamsContainSpecialChars_EncodesQueryParams()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Contracts.GetBondFiltersAsync("SYM&evil=1", "ISSUER/ID");

        var query = getCapture()!.RequestUri!.Query;
        query.Should().Contain("symbol=SYM%26evil%3D1");
        query.Should().Contain("issuerId=ISSUER%2FID");
    }

    [Test]
    public async Task SearchAsync_WhenSymbolContainsSpecialChars_EncodesQueryParam()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("[]");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Contracts.SearchAsync("A B&C=D");

        getCapture()!.RequestUri!.Query.Should().Contain("symbol=A%20B%26C%3DD");
    }

    [Test]
    public async Task GetTradingScheduleAsync_WhenParamsContainSpecialChars_EncodesQueryParams()
    {
        var (mock, getCapture, _) = MockHttpHandler.CapturingSetup("{}");
        using var client = new IBPortalClient(MockHttpHandler.DefaultOptions(), mock.Object);

        await client.Contracts.GetTradingScheduleAsync("ST K", "A&B");

        var query = getCapture()!.RequestUri!.Query;
        query.Should().Contain("assetClass=ST%20K");
        query.Should().Contain("symbol=A%26B");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<HttpMessageHandler> ThrowingHandler(Exception exception)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
        return mock;
    }
}
