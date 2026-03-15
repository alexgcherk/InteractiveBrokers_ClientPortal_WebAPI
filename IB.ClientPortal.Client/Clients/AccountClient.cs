// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Account information and selection endpoints.</summary>
public class AccountClient
{
    private readonly IBPortalHttpClient _http;

    public AccountClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    ///     GET /iserver/accounts — retrieves the list of accounts and sets the trading context.
    ///     Must be called before placing or querying orders.
    /// </summary>
    public Task<AccountsResponse?> GetAccountsAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<AccountsResponse>("iserver/accounts", ct);
    }

    /// <summary>GET /iserver/account/{accountId}/summary — full account summary with buying power, margins, etc.</summary>
    public Task<AccountSummary?> GetSummaryAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<AccountSummary>($"iserver/account/{accountId}/summary", ct);
    }

    /// <summary>GET /iserver/account/pnl/partitioned — daily PnL broken out per account/segment.</summary>
    public Task<PnlPartitionedResponse?> GetPnlAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<PnlPartitionedResponse>("iserver/account/pnl/partitioned", ct);
    }

    /// <summary>GET /iserver/account/mta — Mobile Trading Assistant alert data.</summary>
    public Task<object?> GetMtaAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<object>("iserver/account/mta", ct);
    }

    /// <summary>POST /iserver/account — switch the currently selected account.</summary>
    public Task<object?> SwitchAccountAsync(string accountId, CancellationToken ct = default)
    {
        return _http.PostAsync<object>("iserver/account", new SwitchAccountRequest { AccountId = accountId }, ct);
    }

    /// <summary>GET /one/user — current user information and feature flags.</summary>
    public Task<UserFeatures?> GetUserAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<UserFeatures>("one/user", ct);
    }

    /// <summary>GET /iserver/currency/pairs?currency=USD — all currency pairs for a base currency.</summary>
    public Task<object?> GetCurrencyPairsAsync(string currency = "USD", CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"iserver/currency/pairs?currency={Uri.EscapeDataString(currency)}", ct);
    }

    /// <summary>GET /iserver/exchangerate?source=USD&amp;target=AUD — spot exchange rate.</summary>
    public Task<object?> GetExchangeRateAsync(string source, string target, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"iserver/exchangerate?source={Uri.EscapeDataString(source)}&target={Uri.EscapeDataString(target)}", ct);
    }

    /// <summary>GET /acesws/{accountId}/signatures-and-owners — applicant names on the account.</summary>
    public Task<object?> GetSignaturesAndOwnersAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"acesws/{accountId}/signatures-and-owners", ct);
    }
}