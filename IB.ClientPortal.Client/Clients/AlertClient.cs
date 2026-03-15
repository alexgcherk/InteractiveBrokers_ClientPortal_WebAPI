// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client.Models;

namespace IB.ClientPortal.Client.Clients;

/// <summary>Price alerts CRUD endpoints.</summary>
public class AlertClient
{
    private readonly IBPortalHttpClient _http;

    public AlertClient(IBPortalHttpClient http)
    {
        _http = http;
    }

    /// <summary>GET /iserver/account/{accountId}/alerts — list all alerts for an account.</summary>
    public Task<AlertSummary[]?> GetAlertsAsync(string accountId, CancellationToken ct = default)
    {
        return _http.GetAsync<AlertSummary[]>($"iserver/account/{accountId}/alerts", ct);
    }

    /// <summary>GET /iserver/account/alert/{orderId}?type=Q — full details of a specific alert.</summary>
    public Task<AlertDetail?> GetAlertAsync(long orderId, CancellationToken ct = default)
    {
        return _http.GetAsync<AlertDetail>($"iserver/account/alert/{orderId}?type=Q", ct);
    }

    /// <summary>POST /iserver/account/{accountId}/alert — create or update an alert.</summary>
    public Task<object?> CreateAlertAsync(
        string accountId, CreateAlertRequest request, CancellationToken ct = default)
    {
        return _http.PostAsync<object>($"iserver/account/{accountId}/alert", request, ct);
    }

    /// <summary>
    ///     POST /iserver/account/{accountId}/alert/activate — activate or deactivate an alert.
    /// </summary>
    public Task<object?> SetAlertActiveAsync(
        string accountId, long alertId, bool active, CancellationToken ct = default)
    {
        return _http.PostAsync<object>(
            $"iserver/account/{accountId}/alert/activate",
            new ActivateAlertRequest { AlertId = alertId, AlertActive = active ? 1 : 0 },
            ct);
    }

    /// <summary>
    ///     DELETE /iserver/account/{accountId}/alert/{alertId} — delete an alert.
    ///     Use alertId = 0 to delete all alerts.
    /// </summary>
    public Task<object?> DeleteAlertAsync(
        string accountId, long alertId, CancellationToken ct = default)
    {
        return _http.DeleteAsync<object>($"iserver/account/{accountId}/alert/{alertId}", ct);
    }

    /// <summary>GET /iserver/account/mta — Mobile Trading Assistant alert.</summary>
    public Task<object?> GetMtaAlertAsync(CancellationToken ct = default)
    {
        return _http.GetAsync<object>("iserver/account/mta", ct);
    }
}