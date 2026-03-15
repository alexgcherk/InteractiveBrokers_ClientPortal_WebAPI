// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IB.ClientPortal.Client.Models;

/// <summary>
///     Deserializes a JSON integer OR quoted-string number into <see cref="long" />.
///     IBKR returns <c>conid</c> as an integer for equities but as a string for FX CASH pairs.
/// </summary>
internal sealed class FlexibleLongConverter : JsonConverter<long>
{
    public override long ReadJson(
        JsonReader reader, Type objectType, long existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
            return Convert.ToInt64(reader.Value);
        if (reader.TokenType == JsonToken.String &&
            long.TryParse((string?)reader.Value, out var parsed))
            return parsed;
        // Unexpected token — consume it without error
        JToken.Load(reader);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}

// ── Contracts / SecDef ────────────────────────────────────────────────────────

public sealed class ContractInfo
{
    [JsonProperty("con_id")] public long ConId { get; set; }
    [JsonProperty("symbol")] public string? Symbol { get; set; }
    [JsonProperty("instrument_type")] public string? SecType { get; set; } // "STK", "CASH", etc.
    [JsonProperty("exchange")] public string? Exchange { get; set; }
    [JsonProperty("currency")] public string? Currency { get; set; }
    [JsonProperty("expiry")] public string? Expiry { get; set; }
    [JsonProperty("strike")] public double Strike { get; set; }
    [JsonProperty("right")] public string? Right { get; set; }
    [JsonProperty("cusip")] public string? Cusip { get; set; }
    [JsonProperty("cfi_code")] public string? CfiCode { get; set; }
    [JsonProperty("industry")] public string? Industry { get; set; }
    [JsonProperty("category")] public string? Category { get; set; }
    [JsonProperty("companyName")] public string? CompanyName { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("undConid")] public long UndConid { get; set; }
    [JsonProperty("multiplier")] public string? Multiplier { get; set; }
    [JsonProperty("local_symbol")] public string? LocalSymbol { get; set; }
    [JsonProperty("trading_class")] public string? TradingClass { get; set; }
    [JsonProperty("valid_exchanges")] public string? ValidExchanges { get; set; }
}

public sealed class SecDefSearchResult
{
    /// <summary>
    ///     Contract ID. Note: IBKR returns this as an integer for STK but as a string for CASH pairs.
    ///     Uses <see cref="FlexibleLongConverter" /> to handle both.
    /// </summary>
    [JsonProperty("conid")]
    [JsonConverter(typeof(FlexibleLongConverter))]
    public long Conid { get; set; }

    [JsonProperty("companyHeader")] public string? CompanyHeader { get; set; }
    [JsonProperty("companyName")] public string? CompanyName { get; set; }
    [JsonProperty("symbol")] public string? Symbol { get; set; }

    /// <summary>Root-level security type (present on CASH FX pairs; STK has this in Sections).</summary>
    [JsonProperty("secType")]
    public string? SecType { get; set; }

    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("restricted")] public string? Restricted { get; set; }
    [JsonProperty("fop")] public string? Fop { get; set; }
    [JsonProperty("opt")] public string? Opt { get; set; }
    [JsonProperty("war")] public string? War { get; set; }
    [JsonProperty("sections")] public SecDefSection[]? Sections { get; set; }
}

public sealed class SecDefSection
{
    [JsonProperty("secType")] public string? SecType { get; set; }
    [JsonProperty("months")] public string? Months { get; set; }
    [JsonProperty("symbol")] public string? Symbol { get; set; }
    [JsonProperty("exchange")] public string? Exchange { get; set; }
}

public sealed class StrikesResponse
{
    [JsonProperty("call")] public double[]? Call { get; set; }
    [JsonProperty("put")] public double[]? Put { get; set; }
}

public sealed class SecDefSearchRequest
{
    [JsonProperty("symbol")] public string Symbol { get; set; } = string.Empty;
    [JsonProperty("secType")] public string? SecType { get; set; }
    [JsonProperty("name")] public bool Name { get; set; }
}

public sealed class StocksResponse
{
    // keys are symbol strings; values are arrays of exchange entries
    [JsonExtensionData] public Dictionary<string, JToken>? Stocks { get; set; }
}

public sealed class StockEntry
{
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("chineseName")] public string? ChineseName { get; set; }
    [JsonProperty("assetClass")] public string? AssetClass { get; set; }
    [JsonProperty("contracts")] public ContractEntry[]? Contracts { get; set; }
}

public sealed class ContractEntry
{
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("exchange")] public string? Exchange { get; set; }
    [JsonProperty("isUS")] public bool IsUS { get; set; }
}

public sealed class ContractRulesRequest
{
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("exchange")] public string? Exchange { get; set; }
    [JsonProperty("isBuy")] public bool IsBuy { get; set; }
    [JsonProperty("modifyOrder")] public bool ModifyOrder { get; set; }
    [JsonProperty("orderId")] public long? OrderId { get; set; }
}

public sealed class FutureContract
{
    [JsonProperty("symbol")] public string? Symbol { get; set; }
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("underConid")] public long UnderConid { get; set; }
    [JsonProperty("expiry")] public string? Expiry { get; set; }
    [JsonProperty("ltd")] public string? Ltd { get; set; }
    [JsonProperty("shortName")] public string? ShortName { get; set; }
}