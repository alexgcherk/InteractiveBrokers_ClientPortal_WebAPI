// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Clients;
using IBClientPortal.Client.Generated.Calendar;
using IBClientPortal.Client.Generated.Fyi;

namespace IB.ClientPortal.Client;

/// <summary>
///     Main entry point for the IB Client Portal Web API.
///     <para>
///         Usage:
///         <code>
/// var options = new IBPortalClientOptions { BaseUrl = "https://localhost:5000", AccountId = "DU55124354" };
/// using var client = new IBPortalClient(options);
/// var status = await client.Auth.GetStatusAsync();
/// var accounts = await client.Account.GetAccountsAsync();
/// </code>
///     </para>
/// </summary>
public sealed class IBPortalClient : IDisposable
{
    private readonly IBPortalHttpClient _http;

    // ── Constructors ────────────────────────────────────────────────────────────

    public IBPortalClient(IBPortalClientOptions options)
        : this(options, new IBPortalHttpClient(options))
    {
    }

    /// <summary>Constructor for unit-testing with an injected <see cref="HttpMessageHandler" />.</summary>
    public IBPortalClient(IBPortalClientOptions options, HttpMessageHandler handler)
        : this(options, new IBPortalHttpClient(options, handler))
    {
    }

    private IBPortalClient(IBPortalClientOptions options, IBPortalHttpClient http)
    {
        _http = http;
        DefaultAccountId = options.AccountId;

        Auth = new AuthClient(http);
        Account = new AccountClient(http);
        Orders = new OrderClient(http);
        Portfolio = new PortfolioClient(http);
        MarketData = new MarketDataClient(http);
        Contracts = new ContractClient(http);
        Alerts = new AlertClient(http);
        Performance = new PerformanceClient(http);
        Watchlists = new WatchlistClient(http);

        var apiBaseUrl = options.BaseUrl.TrimEnd('/') + "/v1/api/";
        Fyi = new FyiClient(_http.HttpClient);
        Fyi.BaseUrl = apiBaseUrl;
        Calendar = new CalendarClient(_http.HttpClient);
        Calendar.BaseUrl = apiBaseUrl;
    }

    // ── Hand-crafted domain clients ────────────────────────────────────────────

    /// <summary>Authentication and session management.</summary>
    public AuthClient Auth { get; }

    /// <summary>Account information, PnL, user data.</summary>
    public AccountClient Account { get; }

    /// <summary>Order placement, modification, cancellation and query.</summary>
    public OrderClient Orders { get; }

    /// <summary>Portfolio positions, allocation and cash ledger.</summary>
    public PortfolioClient Portfolio { get; }

    /// <summary>Real-time snapshots, historical bars, scanner.</summary>
    public MarketDataClient MarketData { get; }

    /// <summary>Contract and security definition lookup.</summary>
    public ContractClient Contracts { get; }

    /// <summary>Price alerts CRUD.</summary>
    public AlertClient Alerts { get; }

    /// <summary>Performance Analytics (PA) — portfolio performance, summary, transactions.</summary>
    public PerformanceClient Performance { get; }

    /// <summary>Watchlist management.</summary>
    public WatchlistClient Watchlists { get; }

    // ── NSwag-generated clients (FYI and Calendar from bundled swagger YAMLs) ──

    /// <summary>FYI notifications and delivery options (NSwag-generated).</summary>
    public FyiClient Fyi { get; }

    /// <summary>Event Calendar (NSwag-generated).</summary>
    public CalendarClient Calendar { get; }

    // ── Convenience property ────────────────────────────────────────────────────

    /// <summary>Default account ID passed in options, used for account-scoped calls.</summary>
    public string? DefaultAccountId { get; }

    public void Dispose()
    {
        _http.Dispose();
    }
}