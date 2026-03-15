// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Market data snapshot, historical bars, and scanner endpoints.</summary>
public class MarketDataClient
{
    private readonly IBPortalHttpClient _http;

    public MarketDataClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    ///     GET /iserver/marketdata/snapshot?conids={conids}&amp;fields={fields}
    ///     Returns real-time snapshot data. The first call subscribes; subsequent calls return data.
    /// </summary>
    /// <param name="conids">Comma-separated contract IDs.</param>
    /// <param name="fields">Comma-separated field codes (e.g. "31,84,86"). See MarketDataField constants.</param>
    public Task<MarketDataSnapshot[]?> GetSnapshotAsync(
        string conids, string fields, CancellationToken ct = default)
    {
        return _http.GetAsync<MarketDataSnapshot[]>(
            $"iserver/marketdata/snapshot?conids={conids}&fields={fields}", ct);
    }

    /// <summary>
    ///     GET /iserver/marketdata/history?conid={conid}&amp;period={period}&amp;bar={bar}
    ///     Historical OHLCV bars.
    /// </summary>
    /// <param name="conid">Contract ID.</param>
    /// <param name="period">Time period, e.g. "1d", "5d", "1m", "3m", "1y".</param>
    /// <param name="bar">Bar size, e.g. "1min", "5min", "1h", "1d".</param>
    /// <param name="exchange">Optional exchange override.</param>
    /// <param name="outsideRth">Include data outside regular trading hours.</param>
    public Task<HistoricalDataResponse?> GetHistoryAsync(
        long conid, string period, string bar,
        string? exchange = null, bool outsideRth = false,
        CancellationToken ct = default)
    {
        var url = $"iserver/marketdata/history?conid={conid}&period={Uri.EscapeDataString(period)}&bar={Uri.EscapeDataString(bar)}&outsideRth={outsideRth}";
        if (!string.IsNullOrWhiteSpace(exchange)) url += $"&exchange={Uri.EscapeDataString(exchange)}";
        return _http.GetAsync<HistoricalDataResponse>(url, ct);
    }

    /// <summary>GET /iserver/marketdata/unsubscribeall — cancel all active market data subscriptions.</summary>
    public Task<UnsubscribeAllResponse?> UnsubscribeAllAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<UnsubscribeAllResponse>("iserver/marketdata/unsubscribeall", ct);
    }

    /// <summary>POST /iserver/marketdata/unsubscribe — cancel subscription for a single contract.</summary>
    public Task<object?> UnsubscribeAsync(long conid, CancellationToken ct = default)
    {
        return _http.PostAsync<object>("iserver/marketdata/unsubscribe", new UnsubscribeRequest { Conid = conid }, ct);
    }

    /// <summary>
    ///     GET /md/snapshot?conids={conids}&amp;fields={fields} — alternative snapshot endpoint.
    ///     Returns empty if subscription not established via /iserver/marketdata/snapshot first.
    /// </summary>
    public Task<MarketDataSnapshot[]?> GetAltSnapshotAsync(
        string conids, string fields, CancellationToken ct = default)
    {
        return _http.GetAsync<MarketDataSnapshot[]>($"md/snapshot?conids={conids}&fields={fields}", ct);
    }

    /// <summary>
    ///     GET /md/regsnapshot?conid={conid} — regulatory snapshot. ⚠️ Costs $0.01 per request.
    /// </summary>
    public Task<MarketDataSnapshot?> GetRegSnapshotAsync(long conid, CancellationToken ct = default)
    {
        return _http.GetAsync<MarketDataSnapshot>($"md/regsnapshot?conid={conid}", ct);
    }

    /// <summary>GET /iserver/scanner/params — full scanner configuration (cached 15 min).</summary>
    public Task<object?> GetScannerParamsAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<object>("iserver/scanner/params", ct);
    }

    /// <summary>POST /iserver/scanner/run — execute a market scanner.</summary>
    public Task<ScannerResult?> RunScannerAsync(ScannerRunRequest request, CancellationToken ct = default)
    {
        return _http.PostAsync<ScannerResult>("iserver/scanner/run", request, ct);
    }
}

/// <summary>Commonly used market data field codes for snapshot requests.</summary>
public static class MarketDataFields
{
    public const string Last = "31";
    public const string Bid = "84";
    public const string Ask = "86";
    public const string BidSize = "85";
    public const string AskSize = "88";
    public const string Volume = "87";
    public const string High = "70";
    public const string Low = "71";
    public const string Open = "7295";
    public const string Close = "7296";
    public const string PriorClose = "7741";
    public const string Change = "82";
    public const string ChangePct = "83";
    public const string High52Week = "7293";
    public const string Low52Week = "7294";
    public const string Delta = "7308";
    public const string Gamma = "7309";
    public const string Theta = "7310";
    public const string Vega = "7311";

    /// <summary>A useful default set of fields for equity snapshots.</summary>
    public static readonly string EquityDefault =
        string.Join(",", Last, Bid, Ask, BidSize, AskSize, Volume, High, Low, Open, Close, PriorClose, Change,
            ChangePct);
}