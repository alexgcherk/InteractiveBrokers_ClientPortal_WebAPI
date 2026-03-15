// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace IBClientPortal.Integration.Tests;

public sealed class GatewaySettings
{
    public string BaseUrl { get; set; } = "https://localhost:5000";
    public string AccountId { get; set; } = string.Empty;
    public bool IgnoreSslErrors { get; set; } = true;
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    ///     Optional x-sess-uuid cookie value. Obtain from browser DevTools → Application →
    ///     Cookies → https://localhost:5000 after logging in. Leave empty to let the client
    ///     acquire the cookie automatically.
    /// </summary>
    public string? SessionCookie { get; set; }
}