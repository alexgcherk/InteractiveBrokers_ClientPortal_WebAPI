// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Portfolio positions, allocation, and ledger endpoints.</summary>
public class PortfolioClient
{
    private readonly IBPortalHttpClient _http;

    public PortfolioClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>GET /portfolio/accounts — all accounts with full metadata.</summary>
    public Task<PortfolioAccount[]?> GetAccountsAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<PortfolioAccount[]>("portfolio/accounts", ct);
    }

    /// <summary>GET /portfolio/subaccounts — sub-account list.</summary>
    public Task<PortfolioAccount[]?> GetSubAccountsAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<PortfolioAccount[]>("portfolio/subaccounts", ct);
    }

    /// <summary>GET /portfolio/subaccounts2?page={page} — paginated sub-accounts for advisors with &gt;100 accounts.</summary>
    public Task<object?> GetSubAccountsPagedAsync(int page = 0, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"portfolio/subaccounts2?page={page}", ct);
    }

    /// <summary>GET /portfolio/{accountId}/positions/{pageId} — positions page (0-based).</summary>
    public Task<Position[]?> GetPositionsAsync(string accountId, int page = 0, CancellationToken ct = default)
    {
        return _http.GetAsync<Position[]>($"portfolio/{accountId}/positions/{page}", ct);
    }

    /// <summary>GET /portfolio/{accountId}/positions/first — first positions page shortcut.</summary>
    public Task<Position[]?> GetFirstPositionsAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<Position[]>($"portfolio/{accountId}/positions/first", ct);
    }

    /// <summary>GET /portfolio/{accountId}/position/{conid} — position for a specific contract.</summary>
    public Task<Position[]?> GetPositionByConidAsync(string accountId, long conid, CancellationToken ct = default)
    {
        return _http.GetAsync<Position[]>($"portfolio/{accountId}/position/{conid}", ct);
    }

    /// <summary>GET /portfolio/{accountId}/positions/invalidate — invalidates the server-side position cache.</summary>
    public Task<object?> InvalidatePositionCacheAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"portfolio/{accountId}/positions/invalidate", ct);
    }

    /// <summary>GET /portfolio/{accountId}/allocation — asset class, sector and group allocation breakdown.</summary>
    public Task<Allocation?> GetAllocationAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<Allocation>($"portfolio/{accountId}/allocation", ct);
    }

    /// <summary>GET /portfolio/{accountId}/ledger — cash balances per currency.</summary>
    public Task<Dictionary<string, CurrencyLedger>?> GetLedgerAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<Dictionary<string, CurrencyLedger>>($"portfolio/{accountId}/ledger", ct);
    }

    /// <summary>GET /portfolio/{accountId}/meta — full account metadata (may time out; call GetAccountsAsync first).</summary>
    public Task<object?> GetMetaAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"portfolio/{accountId}/meta", ct);
    }
}