// Copyright (c) 2026 Alex Cherkasov. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using NUnit.Framework;

namespace IBClientPortal.Integration.Tests.Tests;

[TestFixture]
public class WatchlistIntegrationTests : IntegrationTestBase
{
    [OneTimeTearDown]
    public async Task TearDown()
    {
        // Clean up test watchlist if it was created
        try
        {
            await Client.Watchlists.DeleteWatchlistAsync(TestWatchlistId);
        }
        catch
        {
            /* may not exist */
        }
    }

    private const int TestWatchlistId = 99901; // IBKR requires numeric watchlist IDs
    private const string TestWatchlistName = "Integration Test";

    [Test]
    [Order(1)]
    public async Task GetWatchlists_ReturnsSystemAndUserLists()
    {
        var result = await Client.Watchlists.GetWatchlistsAsync();
        result.Should().NotBeNull();
        TestContext.WriteLine($"Watchlists response: {result}");
    }

    [Test]
    [Order(2)]
    public async Task CreateWatchlist_WithAaplAndMsft_Succeeds()
    {
        // Apple=265598, MSFT=272093
        var result = await Client.Watchlists.CreateWatchlistAsync(
            TestWatchlistId, TestWatchlistName, [265598, 272093]);
        TestContext.WriteLine($"Created watchlist: {result}");
    }

    [Test]
    [Order(3)]
    public async Task GetWatchlist_ReturnsCreatedWatchlist()
    {
        var result = await Client.Watchlists.GetWatchlistAsync(TestWatchlistId);
        result.Should().NotBeNull();
        TestContext.WriteLine($"Watchlist {TestWatchlistId}: {result}");
    }

    [Test]
    [Order(4)]
    public async Task DeleteWatchlist_Succeeds()
    {
        var result = await Client.Watchlists.DeleteWatchlistAsync(TestWatchlistId);
        TestContext.WriteLine($"Deleted watchlist: {result}");
    }
}