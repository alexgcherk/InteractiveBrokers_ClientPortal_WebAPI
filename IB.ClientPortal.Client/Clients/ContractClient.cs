// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Contract and security definition lookup endpoints.</summary>
public class ContractClient
{
    private readonly IBPortalHttpClient _http;

    public ContractClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>GET /iserver/contract/{conid}/info — contract details.</summary>
    public Task<ContractInfo?> GetInfoAsync(long conid, CancellationToken ct = default)
    {
        return _http.GetAsync<ContractInfo>($"iserver/contract/{conid}/info", ct);
    }

    /// <summary>GET /iserver/contract/{conid}/info-and-rules — contract info plus order rules.</summary>
    public Task<ContractInfo?> GetInfoAndRulesAsync(long conid, CancellationToken ct = default)
    {
        return _http.GetAsync<ContractInfo>($"iserver/contract/{conid}/info-and-rules", ct);
    }

    /// <summary>POST /iserver/contract/rules — order rules for a given contract/side.</summary>
    public Task<object?> GetRulesAsync(ContractRulesRequest request, CancellationToken ct = default)
    {
        return _http.PostAsync<object>("iserver/contract/rules", request, ct);
    }

    /// <summary>
    ///     GET /iserver/secdef/search?symbol={symbol} — search for contracts by symbol.
    ///     Must be called before strikes/secdef/info for options.
    /// </summary>
    public Task<SecDefSearchResult[]?> SearchAsync(
        string symbol, string? secType = null, bool nameSearch = false,
        CancellationToken ct = default)
    {
        var url = $"iserver/secdef/search?symbol={Uri.EscapeDataString(symbol)}&name={nameSearch}";
        if (!string.IsNullOrWhiteSpace(secType)) url += $"&secType={Uri.EscapeDataString(secType)}";
        return _http.GetAsync<SecDefSearchResult[]>(url, ct);
    }

    /// <summary>GET /iserver/secdef/strikes?conid={conid}&amp;sectype=OPT&amp;month={month} — option strike list.</summary>
    public Task<StrikesResponse?> GetStrikesAsync(
        long conid, string month, string secType = "OPT",
        string? exchange = null, CancellationToken ct = default)
    {
        var url = $"iserver/secdef/strikes?conid={conid}&sectype={Uri.EscapeDataString(secType)}&month={Uri.EscapeDataString(month)}";
        if (!string.IsNullOrWhiteSpace(exchange)) url += $"&exchange={Uri.EscapeDataString(exchange)}";
        return _http.GetAsync<StrikesResponse>(url, ct);
    }

    /// <summary>
    ///     GET /iserver/secdef/info?conid={conid}&amp;sectype=OPT&amp;month={month}&amp;strike={strike}&amp;right={right}
    ///     Derivative secdef (all parameters required for options).
    /// </summary>
    public Task<ContractInfo[]?> GetSecDefInfoAsync(
        long conid, string secType, string? month = null,
        double? strike = null, string? right = null,
        CancellationToken ct = default)
    {
        var url = $"iserver/secdef/info?conid={conid}&sectype={Uri.EscapeDataString(secType)}";
        if (month is not null) url += $"&month={Uri.EscapeDataString(month)}";
        if (strike is not null) url += $"&strike={strike}";
        if (right is not null) url += $"&right={Uri.EscapeDataString(right)}";
        return _http.GetAsync<ContractInfo[]>(url, ct);
    }

    /// <summary>GET /trsrv/secdef?conids={conids} — full secdef with trading rules.</summary>
    public Task<object?> GetSecDefAsync(string conids, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"trsrv/secdef?conids={conids}", ct);
    }

    /// <summary>GET /trsrv/stocks?symbols={symbols} — stock contracts by symbol.</summary>
    public Task<Dictionary<string, object>?> GetStocksAsync(string symbols, CancellationToken ct = default)
    {
        return _http.GetAsync<Dictionary<string, object>>($"trsrv/stocks?symbols={symbols}", ct);
    }

    /// <summary>GET /trsrv/futures?symbols={symbols} — non-expired futures contracts.</summary>
    public Task<Dictionary<string, FutureContract[]>?> GetFuturesAsync(
        string symbols, CancellationToken ct = default)
    {
        return _http.GetAsync<Dictionary<string, FutureContract[]>>($"trsrv/futures?symbols={symbols}", ct);
    }

    /// <summary>GET /trsrv/all-conids?exchange={exchange}&amp;assetClass=STK — all conids on an exchange.</summary>
    public Task<ContractEntry[]?> GetAllConidsAsync(
        string exchange, string assetClass = "STK", CancellationToken ct = default)
    {
        return _http.GetAsync<ContractEntry[]>($"trsrv/all-conids?exchange={Uri.EscapeDataString(exchange)}&assetClass={Uri.EscapeDataString(assetClass)}", ct);
    }

    /// <summary>GET /trsrv/secdef/schedule?assetClass={assetClass}&amp;symbol={symbol} — trading schedule per venue.</summary>
    public Task<object?> GetTradingScheduleAsync(
        string assetClass, string symbol, string? exchange = null,
        CancellationToken ct = default)
    {
        var url = $"trsrv/secdef/schedule?assetClass={Uri.EscapeDataString(assetClass)}&symbol={Uri.EscapeDataString(symbol)}";
        if (!string.IsNullOrWhiteSpace(exchange)) url += $"&exchange={Uri.EscapeDataString(exchange)}";
        return _http.GetAsync<object>(url, ct);
    }

    /// <summary>GET /contract/trading-schedule?conid={conid} — 6-day trading schedule with epoch times.</summary>
    public Task<object?> GetContractTradingScheduleAsync(
        long conid, string? exchange = null, CancellationToken ct = default)
    {
        var url = $"contract/trading-schedule?conid={conid}";
        if (!string.IsNullOrWhiteSpace(exchange)) url += $"&exchange={exchange}";
        return _http.GetAsync<object>(url, ct);
    }

    /// <summary>GET /iserver/contract/{conid}/algos — IB Algo strategies available for a contract.</summary>
    public Task<object?> GetAlgosAsync(
        long conid, string? algos = null, bool addDescription = true, bool addParams = true,
        CancellationToken ct = default)
    {
        var url =
            $"iserver/contract/{conid}/algos?addDescription={(addDescription ? 1 : 0)}&addParams={(addParams ? 1 : 0)}";
        if (!string.IsNullOrWhiteSpace(algos)) url += $"&algos={algos}";
        return _http.GetAsync<object>(url, ct);
    }

    /// <summary>GET /iserver/secdef/bond-filters — bond search filter options.</summary>
    public Task<object?> GetBondFiltersAsync(
        string symbol, string issuerId, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"iserver/secdef/bond-filters?symbol={Uri.EscapeDataString(symbol)}&issuerId={Uri.EscapeDataString(issuerId)}", ct);
    }
}