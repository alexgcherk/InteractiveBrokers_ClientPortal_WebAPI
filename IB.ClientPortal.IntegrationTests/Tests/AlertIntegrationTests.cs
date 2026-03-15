// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class AlertIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetAlerts_ReturnsListOrEmpty()
    {
        var result = await Client.Alerts.GetAlertsAsync(AccountId);

        result.Should().NotBeNull("alerts endpoint should return an array (possibly empty)");
        TestContext.WriteLine($"Alerts count: {result!.Length}");
        foreach (var a in result.Take(3))
            TestContext.WriteLine($"  [{a.OrderId}] {a.AlertName} (active={a.AlertActive})");
    }

    [Test]
    public async Task GetMtaAlert_ReturnsWithoutError()
    {
        var result = await Client.Alerts.GetMtaAlertAsync();
        TestContext.WriteLine($"MTA alert: {result}");
    }
}