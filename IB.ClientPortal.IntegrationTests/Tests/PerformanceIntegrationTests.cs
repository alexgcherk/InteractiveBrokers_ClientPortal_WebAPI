// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class PerformanceIntegrationTests : IntegrationTestBase
{
    // Rate-limited to 1 req / 15 min each — run sparingly in CI

    [Test]
    public async Task GetPerformance_Daily_ReturnsWithoutError()
    {
        // Valid period values: "1M", "3M", "6M", "1Y", "2Y", "3Y", "5Y", "MTD", "YTD"
        var result = await Client.Performance.GetPerformanceAsync([AccountId]);
        TestContext.WriteLine($"PA performance: {result}");
    }

    [Test]
    public async Task GetSummary_ReturnsWithoutError()
    {
        var result = await Client.Performance.GetSummaryAsync([AccountId]);
        TestContext.WriteLine($"PA summary: {result}");
    }

    [Test]
    public async Task GetTransactions_Aapl_ReturnsWithoutError()
    {
        // PA transactions requires specific conids — use AAPL (265598) as a known valid conid
        var result = await Client.Performance.GetTransactionsAsync([AccountId], [265598]);
        TestContext.WriteLine($"PA transactions: {result}");
    }
}