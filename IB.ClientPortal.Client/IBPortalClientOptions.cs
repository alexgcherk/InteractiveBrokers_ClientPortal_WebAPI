// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace IB.ClientPortal.Client;

/// <summary>
///     Configuration options for <see cref="IBPortalClient" />.
/// </summary>
public sealed class IBPortalClientOptions
{
    /// <summary>Base URL of the gateway (default: https://localhost:5000).</summary>
    public string BaseUrl { get; set; } = "https://localhost:5000";

    /// <summary>
    ///     Whether to disable SSL certificate validation. Default is <c>false</c>.
    ///     Set to <c>true</c> only in local development against a self-signed gateway certificate.
    ///     Never enable this in production — it makes the connection vulnerable to MITM attacks.
    /// </summary>
    public bool IgnoreSslErrors { get; set; } = false;

    /// <summary>Default account ID used for account-scoped endpoints.</summary>
    public string? AccountId { get; set; }

    /// <summary>Timeout for individual HTTP requests.</summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    ///     Optional pre-seeded session cookie value (<c>x-sess-uuid</c>).
    ///     The gateway sets this automatically on first contact from a local/authenticated source,
    ///     but you can provide it explicitly if the session was established externally
    ///     (e.g. extracted from a browser DevTools Network tab after logging in).
    ///     Leave null to let the client acquire the cookie automatically via the normal flow.
    /// </summary>
    public string? SessionCookie { get; set; }
}