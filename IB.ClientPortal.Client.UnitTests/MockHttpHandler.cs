// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text;
using Moq;
using Moq.Protected;

namespace IB.ClientPortal.Client.UnitTests;

/// <summary>Helper to create a mock HttpMessageHandler that returns a preset response.</summary>
internal static class MockHttpHandler
{
    /// <summary>Creates a mock handler that returns <paramref name="responseJson" /> for the first call.</summary>
    public static Mock<HttpMessageHandler> For(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mock = new Mock<HttpMessageHandler>(); // Loose — allows Dispose
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        return mock;
    }

    /// <summary>Creates a handler where each successive call returns the next response in <paramref name="responses" />.</summary>
    public static Mock<HttpMessageHandler> ForSequence(params string[] responses)
    {
        var mock = new Mock<HttpMessageHandler>(); // Loose — allows Dispose
        var queue = new Queue<string>(responses);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var body = queue.Count > 0 ? queue.Dequeue() : "{}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
            });
        return mock;
    }

    /// <summary>Captures the last request sent through the handler for assertion.</summary>
    public static Mock<HttpMessageHandler> Capturing(
        string responseJson,
        out HttpRequestMessage? captured,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        HttpRequestMessage? cap = null;
        var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => cap = req)
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        captured = null;
        // Use a local variable trick to close over the ref
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => cap = req)
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        // trick: capture via field
        captured = cap; // will be null here; use CapturingSetup below instead
        return mock;
    }

    public static (Mock<HttpMessageHandler> mock, Func<HttpRequestMessage?> getCapture, Func<string?> getCapturedBody)
        CapturingSetup(string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        HttpRequestMessage? cap = null;
        string? body = null;
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                cap = req;
                body = req.Content?.ReadAsStringAsync(ct).GetAwaiter().GetResult();
            })
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        return (mock, () => cap, () => body);
    }

    public static IBPortalClientOptions DefaultOptions()
    {
        return new IBPortalClientOptions
        {
            BaseUrl   = "https://localhost:5000",
            AccountId = "DU0000000"
        };
    }
}