// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Authentication and session management endpoints.</summary>
public class AuthClient
{
    private readonly IBPortalHttpClient _http;

    public AuthClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>GET /iserver/auth/status — returns current brokerage session state.</summary>
    public Task<AuthStatus?> GetStatusAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<AuthStatus>("iserver/auth/status", ct);
    }

    /// <summary>POST /iserver/auth/ssodh/init — opens (or re-opens) a brokerage session.</summary>
    public Task<AuthStatus?> InitSsoDhAsync(CancellationToken ct = default)
    {
        return _http.PostAsync<AuthStatus>("iserver/auth/ssodh/init", null, ct);
    }

    /// <summary>POST /iserver/reauthenticate — force re-authentication.</summary>
    public Task<ReauthResponse?> ReauthenticateAsync(CancellationToken ct = default)
    {
        return _http.PostAsync<ReauthResponse>("iserver/reauthenticate", null, ct);
    }

    /// <summary>
    ///     GET /tickle — keepalive ping. Must be called at least every 5 minutes to maintain the session.
    /// </summary>
    public Task<TickleResponse?> TickleAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<TickleResponse>("tickle", ct);
    }

    /// <summary>GET /logout — ends the current session.</summary>
    public Task<object?> LogoutAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<object>("logout", ct);
    }

    /// <summary>GET /sso/ping — verifies the gateway is alive (no /v1/api prefix).</summary>
    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        // Ping uses the gateway root, not /v1/api
        try
        {
            var raw = await _http.GetRawAsync("../../../sso/ping", ct).ConfigureAwait(false);
            return raw.Contains("true");
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}