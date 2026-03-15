// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>
///     Order placement, modification, cancellation and query endpoints.
/// </summary>
/// <remarks>
///     IMPORTANT: Call <see cref="AccountClient.GetAccountsAsync" /> before any order operations.
///     After placing an order that returns a warning (<see cref="PlaceOrderResponse.ReplyId" /> is set),
///     you MUST immediately call <see cref="ReplyAsync" /> before making any other request.
/// </remarks>
public class OrderClient
{
    private readonly IBPortalHttpClient _http;

    public OrderClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>GET /iserver/account/orders — live orders. Optionally filter by status.</summary>
    /// <param name="filters">Comma-separated status filters, e.g. "filled,cancelled,inactive".</param>
    public Task<OrdersResponse?> GetOrdersAsync(string? filters = null, CancellationToken ct = default)
    {
        var url = "iserver/account/orders";
        if (!string.IsNullOrWhiteSpace(filters)) url += $"?filters={filters}";
        return _http.GetAsync<OrdersResponse>(url, ct);
    }

    /// <summary>GET /iserver/account/trades — today's filled trades.</summary>
    /// <param name="days">Number of days to look back (default: today only).</param>
    public Task<Trade[]?> GetTradesAsync(int? days = null, CancellationToken ct = default)
    {
        var url = "iserver/account/trades";
        if (days.HasValue) url += $"?days={days}";
        return _http.GetAsync<Trade[]>(url, ct);
    }

    /// <summary>
    ///     POST /iserver/account/{accountId}/orders — place one or more orders.
    ///     The gateway may return a single JSON object or a JSON array — both are handled.
    /// </summary>
    public async Task<PlaceOrderResponse[]?> PlaceOrdersAsync(
        string accountId, PlaceOrderBody[] orders, CancellationToken ct = default)
    {
        var raw = await RawPostAsync(
            $"iserver/account/{accountId}/orders",
            new PlaceOrderRequest { Orders = orders }, ct);
        return ParseOrderResponse(raw);
    }

    /// <summary>POST /iserver/account/{accountId}/orders/whatif — margin/commission impact without placing.</summary>
    public Task<WhatIfResponse?> WhatIfAsync(
        string accountId, PlaceOrderBody[] orders, CancellationToken ct = default)
    {
        return _http.PostAsync<WhatIfResponse>(
            $"iserver/account/{accountId}/orders/whatif",
            new PlaceOrderRequest { Orders = orders },
            ct);
    }

    /// <summary>POST /iserver/account/{accountId}/order/{orderId} — modify an existing order.</summary>
    public async Task<PlaceOrderResponse[]?> ModifyOrderAsync(
        string accountId, long orderId, PlaceOrderBody updatedOrder, CancellationToken ct = default)
    {
        var raw = await RawPostAsync(
            $"iserver/account/{accountId}/order/{orderId}", updatedOrder, ct);
        return ParseOrderResponse(raw);
    }

    /// <summary>DELETE /iserver/account/{accountId}/order/{orderId} — cancel an order. Use orderId = -1 to cancel all.</summary>
    public Task<object?> CancelOrderAsync(
        string accountId, long orderId, CancellationToken ct = default)
    {
        return _http.DeleteAsync<object>($"iserver/account/{accountId}/order/{orderId}", ct);
    }

    /// <summary>GET /iserver/account/order/status/{orderId} — detailed status of a single order.</summary>
    public Task<object?> GetOrderStatusAsync(long orderId, CancellationToken ct = default)
    {
        return _http.GetAsync<object>($"iserver/account/order/status/{orderId}", ct);
    }

    /// <summary>
    ///     POST /iserver/reply/{replyId} — confirm a pending order warning.
    ///     Must be called immediately after receiving a warning reply, before any other request.
    /// </summary>
    public async Task<PlaceOrderResponse[]?> ReplyAsync(
        string replyId, bool confirmed = true, CancellationToken ct = default)
    {
        var raw = await RawPostAsync(
            $"iserver/reply/{replyId}",
            new OrderReplyRequest { Confirmed = confirmed }, ct);
        return ParseOrderResponse(raw);
    }

    /// <summary>
    ///     POST /iserver/questions/suppress — pre-suppress known warning messageIds to avoid reply flow.
    ///     Example messageIds: "o163", "o354".
    /// </summary>
    public Task<object?> SuppressMessagesAsync(
        string[] messageIds, CancellationToken ct = default)
    {
        return _http.PostAsync<object>(
            "iserver/questions/suppress",
            new SuppressMessagesRequest { MessageIds = messageIds },
            ct);
    }

    /// <summary>POST /iserver/questions/suppress/reset — clears all previously suppressed messages.</summary>
    public Task<object?> ResetSuppressedMessagesAsync(CancellationToken ct = default)
    {
        return _http.PostAsync<object>("iserver/questions/suppress/reset", null, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<string> RawPostAsync(string url, object body, CancellationToken ct)
    {
        var content = new StringContent(
            IBPortalHttpClient.Serialize(body),
            Encoding.UTF8, "application/json");
        var response = await _http.HttpClient.PostAsync(url, content, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    private static PlaceOrderResponse[]? ParseOrderResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        json = json.TrimStart();
        if (json.StartsWith('['))
            return IBPortalHttpClient.Deserialize<PlaceOrderResponse[]>(json);
        var single = IBPortalHttpClient.Deserialize<PlaceOrderResponse>(json);
        return single is null ? null : [single];
    }
}