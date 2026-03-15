// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IB.ClientPortal.Client.Models;

// ── Market Data ───────────────────────────────────────────────────────────────

/// <summary>
///     Real-time snapshot entry from /iserver/marketdata/snapshot.
///     Fields are returned as string keys (field code) to object values.
/// </summary>
public sealed class MarketDataSnapshot
{
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("conidEx")] public string? ConidEx { get; set; }
    [JsonProperty("_updated")] public long UpdatedMs { get; set; }

    // Common named fields (the API returns these as numeric string keys)
    [JsonProperty("31")] public string? Last { get; set; }
    [JsonProperty("84")] public string? Bid { get; set; }
    [JsonProperty("86")] public string? Ask { get; set; }
    [JsonProperty("85")] public string? BidSize { get; set; }
    [JsonProperty("88")] public string? AskSize { get; set; }
    [JsonProperty("87")] public string? Volume { get; set; }
    [JsonProperty("70")] public string? High { get; set; }
    [JsonProperty("71")] public string? Low { get; set; }
    [JsonProperty("7295")] public string? Open { get; set; }
    [JsonProperty("7296")] public string? Close { get; set; }
    [JsonProperty("7741")] public string? PriorClose { get; set; }
    [JsonProperty("82")] public string? Change { get; set; }
    [JsonProperty("83")] public string? ChangePct { get; set; }
    [JsonProperty("7293")] public string? High52 { get; set; }
    [JsonProperty("7294")] public string? Low52 { get; set; }
    [JsonProperty("7308")] public string? Delta { get; set; }
    [JsonProperty("7309")] public string? Gamma { get; set; }
    [JsonProperty("7310")] public string? Theta { get; set; }
    [JsonProperty("7311")] public string? Vega { get; set; }

    // All remaining dynamic fields
    [JsonExtensionData] public Dictionary<string, JToken>? AdditionalFields { get; set; }
}

public sealed class HistoricalDataResponse
{
    [JsonProperty("serverId")] public string? ServerId { get; set; }
    [JsonProperty("symbol")] public string? Symbol { get; set; }
    [JsonProperty("text")] public string? Text { get; set; }
    [JsonProperty("priceFactor")] public double PriceFactor { get; set; }
    [JsonProperty("startTime")] public string? StartTime { get; set; }
    [JsonProperty("high")] public string? High { get; set; }
    [JsonProperty("low")] public string? Low { get; set; }
    [JsonProperty("timePeriod")] public string? TimePeriod { get; set; }
    [JsonProperty("barLength")] public int BarLength { get; set; }
    [JsonProperty("mdAvailability")] public string? MdAvailability { get; set; }
    [JsonProperty("shortOutage")] public bool ShortOutage { get; set; }
    [JsonProperty("points")] public int Points { get; set; }
    [JsonProperty("travelTime")] public int TravelTime { get; set; }
    [JsonProperty("data")] public OhlcBar[]? Data { get; set; }
}

public sealed class OhlcBar
{
    [JsonProperty("t")] public long TimeMs { get; set; }
    [JsonProperty("o")] public double Open { get; set; }
    [JsonProperty("h")] public double High { get; set; }
    [JsonProperty("l")] public double Low { get; set; }
    [JsonProperty("c")] public double Close { get; set; }
    [JsonProperty("v")] public double Volume { get; set; }
}

public sealed class UnsubscribeAllResponse
{
    [JsonProperty("unsubscribed")] public bool Unsubscribed { get; set; }
}

public sealed class UnsubscribeRequest
{
    [JsonProperty("conid")] public long Conid { get; set; }
}