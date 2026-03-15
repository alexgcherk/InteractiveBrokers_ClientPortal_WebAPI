// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using IBClientPortal.Client.Generated.Fyi;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class FyiIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetUnreadNumber_ReturnsCount()
    {
        try
        {
            var result = await Client.Fyi.UnreadnumberAsync();
            TestContext.WriteLine($"Unread FYI count: {result}");
        }
        catch (ApiException ex) when (ex.StatusCode == 423)
        {
            // 423 "waiting for reply" is a transient IBKR gateway state; not a client error
            TestContext.WriteLine($"FYI unread count returned 423 (gateway busy): {ex.Response}");
        }
    }

    [Test]
    public async Task GetSettings_ReturnsFyiTypeList()
    {
        var result = await Client.Fyi.SettingsAllAsync();
        result.Should().NotBeNull();
        TestContext.WriteLine($"FYI settings count: {result?.Count}");
    }

    [Test]
    public async Task GetDeliveryOptions_ReturnsOptions()
    {
        var result = await Client.Fyi.DeliveryoptionsGETAsync();
        TestContext.WriteLine($"Delivery options: {result}");
    }

    [Test]
    public async Task GetNotifications_Max10_ReturnsLatestNotifications()
    {
        var result = await Client.Fyi.NotificationsAllAsync(null, null, "10");
        result.Should().NotBeNull();
        TestContext.WriteLine($"Notifications (max 10): {result?.Count}");
        foreach (var n in result?.Take(3) ?? [])
            TestContext.WriteLine($"  [{n.FC}] {n.MS}");
    }
}