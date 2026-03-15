// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace IB.ClientPortal.Client.Models;

// ── Accounts ─────────────────────────────────────────────────────────────────

public sealed class AccountsResponse
{
    [JsonProperty("accounts")] public string[]? Accounts { get; set; }
    [JsonProperty("aliases")] public Dictionary<string, string>? Aliases { get; set; }
    [JsonProperty("selectedAccount")] public string? SelectedAccount { get; set; }
    [JsonProperty("sessionId")] public string? SessionId { get; set; }
}

public sealed class AccountSummary
{
    [JsonProperty("accountType")] public string? AccountType { get; set; }
    [JsonProperty("status")] public string? Status { get; set; }
    [JsonProperty("balance")] public double Balance { get; set; }
    [JsonProperty("SMA")] public double SMA { get; set; }
    [JsonProperty("buyingPower")] public double BuyingPower { get; set; }
    [JsonProperty("availableFunds")] public double AvailableFunds { get; set; }
    [JsonProperty("excessLiquidity")] public double ExcessLiquidity { get; set; }
    [JsonProperty("netLiquidationValue")] public double NetLiquidation { get; set; }
    [JsonProperty("equityWithLoanValue")] public double EquityWithLoanValue { get; set; }
    [JsonProperty("regTLoan")] public double RegTLoan { get; set; }
    [JsonProperty("regTMargin")] public double RegTMargin { get; set; }
    [JsonProperty("initialMargin")] public double InitialMargin { get; set; }
    [JsonProperty("maintenanceMargin")] public double MaintenanceMargin { get; set; }
    [JsonProperty("totalCashValue")] public double TotalCashValue { get; set; }
    [JsonProperty("accruedInterest")] public double AccruedInterest { get; set; }
    [JsonProperty("cashBalances")] public CashBalance[]? CashBalances { get; set; }
}

public sealed class AccountValue
{
    [JsonProperty("amount")] public double Amount { get; set; }
    [JsonProperty("currency")] public string? Currency { get; set; }
}

public sealed class CashBalance
{
    [JsonProperty("currency")] public string? Currency { get; set; }
    [JsonProperty("balance")] public double Balance { get; set; }
    [JsonProperty("settledCash")] public double SettledCash { get; set; }
}

public sealed class PnlPartitionedResponse
{
    [JsonProperty("upnl")] public Dictionary<string, AccountPnl>? Upnl { get; set; }
}

public sealed class AccountPnl
{
    [JsonProperty("rowType")] public int RowType { get; set; }
    [JsonProperty("dpl")] public double Dpl { get; set; }
    [JsonProperty("nl")] public double Nl { get; set; }
    [JsonProperty("upl")] public double Upl { get; set; }
    [JsonProperty("el")] public double El { get; set; }
    [JsonProperty("mv")] public double Mv { get; set; }
}

public sealed class UserFeatures
{
    [JsonProperty("username")] public string? Username { get; set; }
    [JsonProperty("features")] public Dictionary<string, object>? Features { get; set; }
}

public sealed class SwitchAccountRequest
{
    [JsonProperty("acctId")] public string AccountId { get; set; } = string.Empty;
}