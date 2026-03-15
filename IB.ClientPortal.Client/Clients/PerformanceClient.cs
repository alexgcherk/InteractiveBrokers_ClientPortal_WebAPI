// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Performance Analytics (PA) endpoints. All are POST. Rate-limited to 1 req / 15 min.</summary>
public class PerformanceClient
{
    private readonly IBPortalHttpClient _http;

    public PerformanceClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>POST /pa/performance — portfolio performance time series.</summary>
    /// <param name="accountIds">Account IDs to include.</param>
    /// <param name="period">Look-back period. Valid values: "1M", "3M", "6M", "1Y", "2Y", "3Y", "5Y", "MTD", "YTD".</param>
    public Task<object?> GetPerformanceAsync(
        string[] accountIds, string period = "1M", CancellationToken ct = default)
    {
        return _http.PostAsync<object>("pa/performance",
            new PaPerformanceRequest { AccountIds = accountIds, Period = period }, ct);
    }

    /// <summary>POST /pa/summary — portfolio summary by asset class.</summary>
    public Task<object?> GetSummaryAsync(
        string[] accountIds, CancellationToken ct = default)
    {
        return _http.PostAsync<object>("pa/summary",
            new PaSummaryRequest { AccountIds = accountIds }, ct);
    }

    /// <summary>POST /pa/transactions — transaction history for specific contracts.</summary>
    /// <param name="conids">Contract IDs to fetch transactions for. Required.</param>
    /// <param name="currency">Report currency (default: "USD").</param>
    public Task<object?> GetTransactionsAsync(
        string[] accountIds, long[] conids, string currency = "USD",
        CancellationToken ct = default)
    {
        return _http.PostAsync<object>("pa/transactions",
            new PaTransactionsRequest { AccountIds = accountIds, Currency = currency, Conids = conids }, ct);
    }
}