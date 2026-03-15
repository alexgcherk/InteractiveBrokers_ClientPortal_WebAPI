// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace IB.ClientPortal.Client.Clients;

/// <summary>Watchlist management endpoints.</summary>
public class WatchlistClient
{
    private readonly IBPortalHttpClient _http;

    public WatchlistClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>GET /iserver/watchlists — retrieve all watchlists (system and user-defined).</summary>
    public Task<object?> GetWatchlistsAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<object>("iserver/watchlists", ct);
    }

    /// <summary>POST /iserver/watchlist — create a watchlist. The <paramref name="id" /> must be a positive integer.</summary>
    public Task<object?> CreateWatchlistAsync(int id, string name, long[] conids, CancellationToken ct = default)
    {
        return _http.PostAsync<object>("iserver/watchlist", new
        {
            id,
            name,
            rows = conids.Select(c => new { C = c, T = "1" }).ToArray()
        }, ct);
    }

    /// <summary>GET /iserver/watchlist?Id={id} — get a specific watchlist.</summary>
    public Task<object?> GetWatchlistAsync(int id, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"iserver/watchlist?Id={id}", ct);
    }

    /// <summary>DELETE /iserver/watchlist?Id={id} — delete a watchlist.</summary>
    public Task<object?> DeleteWatchlistAsync(int id, CancellationToken ct = default)
    {
        return _http.DeleteAsync<object>($"iserver/watchlist?Id={id}", ct);
    }
}