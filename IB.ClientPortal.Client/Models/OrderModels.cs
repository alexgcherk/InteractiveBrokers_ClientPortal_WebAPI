// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace IB.ClientPortal.Client.Models;

// ── Orders ────────────────────────────────────────────────────────────────────

public sealed class OrdersResponse
{
    [JsonProperty("orders")] public Order[]? Orders { get; set; }
    [JsonProperty("snapshot")] public bool Snapshot { get; set; }
}

public sealed class Order
{
    [JsonProperty("orderId")] public long OrderId { get; set; }
    [JsonProperty("acct")] public string? Account { get; set; }
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("ticker")] public string? Ticker { get; set; }
    [JsonProperty("secType")] public string? SecType { get; set; }
    [JsonProperty("orderType")] public string? OrderType { get; set; }
    [JsonProperty("side")] public string? Side { get; set; }
    [JsonProperty("totalSize")] public double TotalSize { get; set; }
    [JsonProperty("remainingQuantity")] public double RemainingQuantity { get; set; }
    [JsonProperty("filledQuantity")] public double FilledQuantity { get; set; }
    [JsonProperty("price")] public double Price { get; set; }
    [JsonProperty("auxPrice")] public double AuxPrice { get; set; }
    [JsonProperty("status")] public string? Status { get; set; }
    [JsonProperty("tif")] public string? Tif { get; set; }
    [JsonProperty("lastExecutionTime_r")] public long LastExecutionTimeMs { get; set; }
    [JsonProperty("bgColor")] public string? BgColor { get; set; }
    [JsonProperty("fgColor")] public string? FgColor { get; set; }
}

public sealed class Trade
{
    [JsonProperty("execution_id")] public string? ExecutionId { get; set; }
    [JsonProperty("symbol")] public string? Symbol { get; set; }
    [JsonProperty("side")] public string? Side { get; set; }
    [JsonProperty("order_description")] public string? OrderDescription { get; set; }
    [JsonProperty("trade_time")] public string? TradeTime { get; set; }
    [JsonProperty("trade_time_r")] public long TradeTimeMs { get; set; }
    [JsonProperty("size")] public double Size { get; set; }
    [JsonProperty("price")] public string? Price { get; set; }
    [JsonProperty("submitter")] public string? Submitter { get; set; }
    [JsonProperty("exchange")] public string? Exchange { get; set; }
    [JsonProperty("comission")] public double Commission { get; set; }
    [JsonProperty("net_amount")] public double NetAmount { get; set; }
    [JsonProperty("account")] public string? Account { get; set; }
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("sec_type")] public string? SecType { get; set; }
    [JsonProperty("listingExchange")] public string? ListingExchange { get; set; }
}

/// <summary>Single order to place inside <see cref="PlaceOrderRequest" />.</summary>
public sealed class PlaceOrderBody
{
    [JsonProperty("acctId")] public string? AccountId { get; set; }
    [JsonProperty("conid")] public long Conid { get; set; }
    [JsonProperty("secType")] public string? SecType { get; set; }
    [JsonProperty("orderType")] public string OrderType { get; set; } = "LMT";
    [JsonProperty("side")] public string Side { get; set; } = "BUY";
    [JsonProperty("quantity")] public double Quantity { get; set; }
    [JsonProperty("price")] public double? Price { get; set; }
    [JsonProperty("auxPrice")] public double? AuxPrice { get; set; }
    [JsonProperty("tif")] public string Tif { get; set; } = "DAY";
    [JsonProperty("referrer")] public string? Referrer { get; set; }
    [JsonProperty("outsideRTH")] public bool OutsideRTH { get; set; }
    [JsonProperty("useAdaptive")] public bool UseAdaptive { get; set; }
    [JsonProperty("cOID")] public string? ClientOrderId { get; set; }
    [JsonProperty("parentId")] public string? ParentId { get; set; }
    [JsonProperty("listingExchange")] public string? Exchange { get; set; }
    [JsonProperty("manualIndicator")] public bool? ManualIndicator { get; set; }
    [JsonProperty("extOperator")] public string? ExtOperator { get; set; }
}

public sealed class PlaceOrderRequest
{
    [JsonProperty("orders")] public PlaceOrderBody[] Orders { get; set; } = [];
}

/// <summary>Response from a successful order placement.</summary>
public sealed class PlaceOrderResponse
{
    [JsonProperty("order_id")] public string? OrderId { get; set; }

    [JsonProperty("order_status")] public string? OrderStatus { get; set; }

    // When a warning is returned instead of a direct confirmation:
    [JsonProperty("id")] public string? ReplyId { get; set; }
    [JsonProperty("message")] public string[]? Message { get; set; }
}

public sealed class OrderReplyRequest
{
    [JsonProperty("confirmed")] public bool Confirmed { get; set; } = true;
}

public sealed class SuppressMessagesRequest
{
    [JsonProperty("messageIds")] public string[] MessageIds { get; set; } = [];
}

public sealed class WhatIfResponse
{
    [JsonProperty("amount")] public WhatIfAmount? Amount { get; set; }
    [JsonProperty("equity")] public WhatIfAmount? Equity { get; set; }
    [JsonProperty("initial")] public WhatIfAmount? Initial { get; set; }
    [JsonProperty("maintenance")] public WhatIfAmount? Maintenance { get; set; }
    [JsonProperty("warn")] public string? Warn { get; set; }
    [JsonProperty("error")] public string? Error { get; set; }
}

public sealed class WhatIfAmount
{
    [JsonProperty("amount")] public string? Amount { get; set; }
    [JsonProperty("change")] public string? Change { get; set; }
    [JsonProperty("value")] public string? Value { get; set; }
}