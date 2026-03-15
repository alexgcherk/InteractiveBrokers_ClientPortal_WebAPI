// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace IB.ClientPortal.Client.Models;

// ── Alerts ────────────────────────────────────────────────────────────────────

public sealed class AlertSummary
{
    [JsonProperty("order_id")] public long OrderId { get; set; }
    [JsonProperty("account")] public string? Account { get; set; }
    [JsonProperty("alert_name")] public string? AlertName { get; set; }
    [JsonProperty("alert_active")] public int AlertActive { get; set; }
    [JsonProperty("order_time")] public string? OrderTime { get; set; }
    [JsonProperty("alert_email_mfa")] public string? EmailMfa { get; set; }
    [JsonProperty("itws_orders_only")] public int ItwsOrdersOnly { get; set; }
    [JsonProperty("tws_orders_only")] public int TwsOrdersOnly { get; set; }
    [JsonProperty("alert_message")] public string? Message { get; set; }
}

public sealed class AlertDetail
{
    [JsonProperty("account")] public string? Account { get; set; }
    [JsonProperty("order_id")] public long OrderId { get; set; }
    [JsonProperty("alert_name")] public string? AlertName { get; set; }
    [JsonProperty("alert_active")] public int AlertActive { get; set; }
    [JsonProperty("alert_repeatable")] public int AlertRepeatable { get; set; }
    [JsonProperty("alert_email")] public string? Email { get; set; }
    [JsonProperty("alert_send_message")] public int SendMessage { get; set; }
    [JsonProperty("tif")] public string? Tif { get; set; }
    [JsonProperty("conditions")] public AlertCondition[]? Conditions { get; set; }
}

public sealed class AlertCondition
{
    [JsonProperty("type")] public int Type { get; set; }
    [JsonProperty("conidex")] public string? ConidEx { get; set; }
    [JsonProperty("operator")] public string? Operator { get; set; }
    [JsonProperty("value")] public string? Value { get; set; }
    [JsonProperty("logicBind")] public string? LogicBind { get; set; }
    [JsonProperty("timeZone")] public string? TimeZone { get; set; }
}

public sealed class CreateAlertRequest
{
    [JsonProperty("order_id")] public long? OrderId { get; set; }
    [JsonProperty("alert_name")] public string AlertName { get; set; } = string.Empty;
    [JsonProperty("alert_active")] public int AlertActive { get; set; } = 1;
    [JsonProperty("alert_repeatable")] public int AlertRepeatable { get; set; }
    [JsonProperty("alert_email")] public string? Email { get; set; }
    [JsonProperty("alert_send_message")] public int SendMessage { get; set; } = 1;
    [JsonProperty("tif")] public string Tif { get; set; } = "GTC";
    [JsonProperty("conditions")] public AlertCondition[]? Conditions { get; set; }
}

public sealed class ActivateAlertRequest
{
    [JsonProperty("alertId")] public long AlertId { get; set; }
    [JsonProperty("alertActive")] public int AlertActive { get; set; }
}

// ── Performance Analytics ─────────────────────────────────────────────────────

public sealed class PaPerformanceRequest
{
    [JsonProperty("acctIds")] public string[] AccountIds { get; set; } = [];

    /// <summary>Valid values: "1M", "3M", "6M", "1Y", "2Y", "3Y", "5Y", "MTD", "YTD".</summary>
    [JsonProperty("period")]
    public string Period { get; set; } = "1M";
}

public sealed class PaSummaryRequest
{
    [JsonProperty("acctIds")] public string[] AccountIds { get; set; } = [];
}

public sealed class PaTransactionsRequest
{
    [JsonProperty("acctIds")] public string[] AccountIds { get; set; } = [];
    [JsonProperty("currency")] public string Currency { get; set; } = "USD";

    /// <summary>Contract IDs to include. Required by the gateway.</summary>
    [JsonProperty("conids")]
    public long[] Conids { get; set; } = [];
}

// ── Scanner ───────────────────────────────────────────────────────────────────

public sealed class ScannerRunRequest
{
    [JsonProperty("instrument")] public string Instrument { get; set; } = string.Empty;
    [JsonProperty("type")] public string Type { get; set; } = string.Empty;

    /// <summary>Must always be serialized as an array (even empty) — the gateway rejects missing filter.</summary>
    [JsonProperty("filter")]
    public ScannerFilter[] Filter { get; set; } = [];

    [JsonProperty("location")] public string? Location { get; set; }
    [JsonProperty("size")] public string Size { get; set; } = "25";
}

public sealed class ScannerFilter
{
    [JsonProperty("code")] public string? Code { get; set; }
    [JsonProperty("value")] public object? Value { get; set; }
}

public sealed class ScannerResult
{
    [JsonProperty("contracts")] public ScannerContract[]? Contracts { get; set; }
    [JsonProperty("scan_type_list")] public object? ScanTypeList { get; set; }
}

public sealed class ScannerContract
{
    [JsonProperty("server_id")] public string? ServerId { get; set; }
    [JsonProperty("column_name")] public string? ColumnName { get; set; }
    [JsonProperty("symbol")] public string? Symbol { get; set; }
    [JsonProperty("conidEx")] public string? ConidEx { get; set; }
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("availExch")] public string? AvailExch { get; set; }
    [JsonProperty("companyName")] public string? CompanyName { get; set; }
}