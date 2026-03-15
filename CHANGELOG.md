# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-03-15

### Added
- `IBPortalClient` — single entry point with typed domain clients for all IB Gateway endpoints
- `AuthClient` — session management: status, SSO init, re-auth, tickle keepalive, logout, ping
- `AccountClient` — accounts list, summary, PnL, user info, account switching, currency pairs and exchange rates
- `OrderClient` — order placement, modification, cancellation, live order/trade query, what-if simulation, warning reply flow and message suppression
- `PortfolioClient` — positions (paged), allocation, cash ledger and account metadata
- `MarketDataClient` — real-time snapshots, regulatory snapshots, historical OHLCV bars, market scanner and `MarketDataFields` constants
- `ContractClient` — symbol search, contract info, option strikes, secdef lookup, trading schedules and bond filters; all string parameters are URL-encoded
- `AlertClient` — full CRUD for price alerts and MTA alert access
- `PerformanceClient` — Performance Analytics: performance time series, portfolio summary and transaction history
- `WatchlistClient` — watchlist create, read and delete
- `FyiClient` and `CalendarClient` — NSwag-generated clients for FYI notifications and event calendar
- `IBPortalClientOptions` — typed configuration with safe defaults (`IgnoreSslErrors = false`)
- Automatic TLS bypass for loopback hosts (`localhost`, `127.x.x.x`, `::1`) — no manual `IgnoreSslErrors` needed in local development
- 71 unit tests (Moq + NUnit + FluentAssertions) including security scenarios
- MIT License
