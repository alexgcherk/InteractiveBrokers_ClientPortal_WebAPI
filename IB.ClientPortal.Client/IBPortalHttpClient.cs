// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace IB.ClientPortal.Client;

/// <summary>
///     Low-level HTTP wrapper for the IB Client Portal Gateway.
///     Handles SSL bypass, persistent cookie session (including rolling <c>x-sess-uuid</c>),
///     base-path construction and JSON serialization.
///     <para>
///         The gateway issues a new <c>x-sess-uuid</c> cookie with every response. The shared
///         <see cref="CookieContainer" /> automatically stores and resends the latest value,
///         so no manual cookie tracking is needed.
///     </para>
/// </summary>
public class IBPortalHttpClient : IDisposable
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    // Single CookieContainer shared with the HttpClientHandler so that
    // Set-Cookie headers (including rolling x-sess-uuid) are automatically
    // stored and resent on every subsequent request.
    private readonly CookieContainer _cookies;

    public IBPortalHttpClient(IBPortalClientOptions options)
    {
        _cookies = new CookieContainer();

        // Pre-seed a session cookie if one was provided (Scenario B: external session).
        // In Scenario A (same-host gateway) the cookie is acquired automatically on
        // first contact and all rolling updates are handled by CookieContainer.
        if (!string.IsNullOrWhiteSpace(options.SessionCookie))
            _cookies.Add(new Uri(options.BaseUrl), new Cookie("x-sess-uuid", options.SessionCookie));

        HttpClient = CreateHttpClient(BuildHandler(options, _cookies), options);
    }

    /// <summary>Constructor for unit-testing with an injected <see cref="HttpMessageHandler" />.</summary>
    public IBPortalHttpClient(IBPortalClientOptions options, HttpMessageHandler handler)
    {
        // Unit-test path: the mock handler manages its own cookie behaviour (none).
        _cookies = new CookieContainer();
        HttpClient = CreateHttpClient(handler, options);
    }

    internal HttpClient HttpClient { get; }

    public void Dispose()
    {
        HttpClient.Dispose();
    }

    // ── HTTP helpers ────────────────────────────────────────────────────────

    public Task<T?> GetAsync<T>(string relativeUrl, CancellationToken ct = default)
    {
        return SendAsync<T>(HttpMethod.Get, relativeUrl, null, ct);
    }

    public Task<T?> PostAsync<T>(string relativeUrl, object? body = null, CancellationToken ct = default)
    {
        return SendAsync<T>(HttpMethod.Post, relativeUrl, body, ct);
    }

    public Task<T?> PutAsync<T>(string relativeUrl, object? body = null, CancellationToken ct = default)
    {
        return SendAsync<T>(HttpMethod.Put, relativeUrl, body, ct);
    }

    public Task<T?> DeleteAsync<T>(string relativeUrl, CancellationToken ct = default)
    {
        return SendAsync<T>(HttpMethod.Delete, relativeUrl, null, ct);
    }

    public async Task<T?> SendAsync<T>(HttpMethod method, string relativeUrl, object? body,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(method, relativeUrl);
        if (body is not null)
        {
            var json = JsonConvert.SerializeObject(body, JsonSettings);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await HttpClient.SendAsync(request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(content)) return default;
        return JsonConvert.DeserializeObject<T>(content, JsonSettings);
    }

    public async Task<string> GetRawAsync(string relativeUrl, CancellationToken ct = default)
    {
        using var response = await HttpClient.GetAsync(relativeUrl, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    /// <summary>Sends a GET and returns the raw JToken for dynamic/untyped responses.</summary>
    public async Task<JToken?> GetJTokenAsync(string relativeUrl, CancellationToken ct = default)
    {
        var raw = await GetRawAsync(relativeUrl, ct).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(raw) ? null : JToken.Parse(raw);
    }

    public static string Serialize(object body)
    {
        return JsonConvert.SerializeObject(body, JsonSettings);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, JsonSettings);
    }

    // ── Builders ─────────────────────────────────────────────────────────────

    private static HttpClientHandler BuildHandler(IBPortalClientOptions options, CookieContainer cookies)
    {
        var handler = new HttpClientHandler { CookieContainer = cookies, UseCookies = true };
        if (ShouldBypassSslValidation(options))
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        return handler;
    }

    /// <summary>
    ///     Returns <c>true</c> when SSL certificate validation should be skipped.
    ///     Validation is bypassed when <see cref="IBPortalClientOptions.IgnoreSslErrors" /> is set,
    ///     or automatically when the target host is a loopback address
    ///     (<c>localhost</c>, <c>127.0.0.1</c>, <c>::1</c>, etc.) — the IB gateway always uses a
    ///     self-signed certificate in local deployments.
    /// </summary>
    internal static bool ShouldBypassSslValidation(IBPortalClientOptions options)
    {
        if (options.IgnoreSslErrors) return true;

        if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return true;
            if (IPAddress.TryParse(uri.Host, out var ip) && IPAddress.IsLoopback(ip)) return true;
        }

        return false;
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler, IBPortalClientOptions options)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/v1/api/"),
            Timeout = options.RequestTimeout
        };
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // The IBKR gateway rejects requests with no User-Agent (returns 403).
        // Use a realistic browser UA so the gateway treats us as a standard web client.
        http.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        return http;
    }
}