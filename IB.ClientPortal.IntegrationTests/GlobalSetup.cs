// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IB.ClientPortal.Client;
using IBClientPortal.Client;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests;

/// <summary>
///     Assembly-level one-time setup. Creates a single <see cref="IBPortalClient" /> shared by all
///     integration test fixtures and ensures the brokerage session is open before any test runs.
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    public static IBPortalClient Client { get; private set; } = null!;
    public static GatewaySettings Settings { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task SetUpAsync()
    {
        Settings = LoadSettings();

        Client = new IBPortalClient(new IBPortalClientOptions
        {
            BaseUrl = Settings.BaseUrl,
            AccountId = Settings.AccountId,
            IgnoreSslErrors = Settings.IgnoreSslErrors,
            RequestTimeout = TimeSpan.FromSeconds(Settings.RequestTimeoutSeconds),
            SessionCookie = string.IsNullOrWhiteSpace(Settings.SessionCookie) ? null : Settings.SessionCookie
        });

        // Verify gateway is reachable
        var status = await Client.Auth.GetStatusAsync();
        TestContext.Progress.WriteLine(
            $"[GlobalSetup] Auth status: authenticated={status?.Authenticated}, established={status?.Established}");

        if (status?.Authenticated != true || status.Established != true)
        {
            TestContext.Progress.WriteLine("[GlobalSetup] Session not established — calling ssodh/init...");
            await Client.Auth.InitSsoDhAsync();
            await Task.Delay(2000);
        }

        // Keepalive ping
        await Client.Auth.TickleAsync();

        // Pre-flight: /iserver/accounts must be called before order operations
        var accounts = await Client.Account.GetAccountsAsync();
        TestContext.Progress.WriteLine($"[GlobalSetup] Accounts loaded: {string.Join(", ", accounts?.Accounts ?? [])}");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Client?.Dispose();
    }

    // ── Config loader ──────────────────────────────────────────────────────────

    private static GatewaySettings LoadSettings()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.integration.json", false, false)
            .AddJsonFile("appsettings.integration.local.json", true, false)
            .Build();

        var settings = new GatewaySettings();
        config.GetSection("Gateway").Bind(settings);
        return settings;
    }
}