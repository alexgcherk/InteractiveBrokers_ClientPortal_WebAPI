// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IB.ClientPortal.Client.Models;

// ── Portfolio ─────────────────────────────────────────────────────────────────

public sealed class PortfolioAccount
{
    [JsonProperty("id")] public string? Id { get; set; }
    [JsonProperty("accountId")] public string? AccountId { get; set; }
    [JsonProperty("accountVan")] public string? AccountVan { get; set; }
    [JsonProperty("accountTitle")] public string? AccountTitle { get; set; }
    [JsonProperty("displayName")] public string? DisplayName { get; set; }
    [JsonProperty("accountAlias")] public string? AccountAlias { get; set; }
    [JsonProperty("currency")] public string? Currency { get; set; }
    [JsonProperty("type")] public string? Type { get; set; }
    [JsonProperty("tradingType")] public string? TradingType { get; set; }
    [JsonProperty("clearing")] public string? Clearing { get; set; }
    [JsonProperty("status")] public string? Status { get; set; }
    [JsonProperty("covestor")] public bool Covestor { get; set; }
    [JsonProperty("noClientTrading")] public bool NoClientTrading { get; set; }
    [JsonProperty("hasChildAccounts")] public bool HasChildAccounts { get; set; }
}

public sealed class Position
{
    [JsonProperty("acctId")] public string? AccountId { get; set; }
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("contractDesc")] public string? ContractDesc { get; set; }
    [JsonProperty("position")] public double Quantity { get; set; }
    [JsonProperty("mktPrice")] public double MarketPrice { get; set; }
    [JsonProperty("mktValue")] public double MarketValue { get; set; }
    [JsonProperty("avgCost")] public double AvgCost { get; set; }
    [JsonProperty("avgPrice")] public double AvgPrice { get; set; }
    [JsonProperty("unrealizedPnl")] public double UnrealizedPnl { get; set; }
    [JsonProperty("realizedPnl")] public double RealizedPnl { get; set; }
    [JsonProperty("currency")] public string? Currency { get; set; }
    [JsonProperty("ticker")] public string? Ticker { get; set; }
    [JsonProperty("secType")] public string? SecType { get; set; }
    [JsonProperty("fullName")] public string? FullName { get; set; }
    [JsonProperty("listingExchange")] public string? ListingExchange { get; set; }
    [JsonProperty("pageId")] public string? PageId { get; set; }
}

public sealed class Allocation
{
    [JsonProperty("assetClass")] public AllocationBreakdown? AssetClass { get; set; }
    [JsonProperty("sector")] public AllocationBreakdown? Sector { get; set; }
    [JsonProperty("group")] public AllocationBreakdown? Group { get; set; }
}

public sealed class AllocationBreakdown
{
    [JsonProperty("long")] public Dictionary<string, double>? Long { get; set; }
    [JsonProperty("short")] public Dictionary<string, double>? Short { get; set; }
}

public sealed class Ledger
{
    // keys are currency codes e.g. "USD", "BASE"
    [JsonExtensionData] public Dictionary<string, JToken>? Currencies { get; set; }
}

public sealed class CurrencyLedger
{
    [JsonProperty("commoditymarketvalue")] public double CommodityMarketValue { get; set; }
    [JsonProperty("futuremarketvalue")] public double FutureMarketValue { get; set; }
    [JsonProperty("settledcash")] public double SettledCash { get; set; }
    [JsonProperty("exchangerate")] public double ExchangeRate { get; set; }
    [JsonProperty("cashbalance")] public double CashBalance { get; set; }
    [JsonProperty("realizedpnl")] public double RealizedPnl { get; set; }
    [JsonProperty("unrealizedpnl")] public double UnrealizedPnl { get; set; }
    [JsonProperty("netliquidationvalue")] public double NetLiquidationValue { get; set; }
    [JsonProperty("key")] public string? Key { get; set; }
    [JsonProperty("timestamp")] public long Timestamp { get; set; }
    [JsonProperty("currency")] public string? Currency { get; set; }
    [JsonProperty("severity")] public int Severity { get; set; }
}