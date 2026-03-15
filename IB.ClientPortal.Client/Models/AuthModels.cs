// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace IB.ClientPortal.Client.Models;

// ── Auth ─────────────────────────────────────────────────────────────────────

public sealed class AuthStatus
{
    [JsonProperty("authenticated")] public bool Authenticated { get; set; }
    [JsonProperty("established")] public bool Established { get; set; }
    [JsonProperty("competing")] public bool Competing { get; set; }
    [JsonProperty("connected")] public bool Connected { get; set; }
    [JsonProperty("MAC")] public string? MAC { get; set; }
    [JsonProperty("serverInfo")] public ServerInfo? ServerInfo { get; set; }
}

public sealed class ServerInfo
{
    [JsonProperty("serverName")] public string? ServerName { get; set; }
    [JsonProperty("serverVersion")] public string? ServerVersion { get; set; }
}

public sealed class TickleResponse
{
    [JsonProperty("session")] public string? Session { get; set; }
    [JsonProperty("ssoExpires")] public int SsoExpires { get; set; }
    [JsonProperty("collission")] public bool Collision { get; set; }
    [JsonProperty("userId")] public int UserId { get; set; }
}

public sealed class ReauthResponse
{
    [JsonProperty("message")] public string? Message { get; set; }
}