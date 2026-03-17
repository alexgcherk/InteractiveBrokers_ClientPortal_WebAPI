# Contributing to InteractiveBrokers\_ClientPortal\_WebAPI

Thank you for your interest in contributing.

This project aims to provide a clean, reliable, and maintainable .NET client for the Interactive Brokers Client Portal Web API. Contributions are welcome, especially those that improve reliability, documentation, test coverage, and developer experience.

## Before You Start

Please take a moment to review:

* `README.md`
* `SECURITY.md`
* open issues and pull requests

For security-related issues, please **do not open a public issue**. Follow the instructions in `SECURITY.md`.

## Ways to Contribute

Contributions are welcome in areas such as:

* bug fixes
* test coverage improvements
* documentation updates
* examples and usage samples
* API consistency improvements
* performance improvements
* refactoring that improves maintainability without breaking behavior

Please keep changes focused and avoid mixing unrelated work in the same pull request.

## Development Setup

1. Fork the repository
2. Clone your fork
3. Create a feature branch from `main`
4. Restore dependencies
5. Build the solution
6. Run the tests

Example:

```bash
git clone https://github.com/YOUR\_USERNAME/InteractiveBrokers\_ClientPortal\_WebAPI.git
cd InteractiveBrokers\_ClientPortal\_WebAPI
git checkout -b feature/my-change
dotnet restore
dotnet build
dotnet test
```

## Branching

Please do not commit directly to `main`.

Create a branch with a descriptive name, for example:

* `fix/session-timeout-handling`
* `docs/update-authentication-example`
* `test/add-coverage-for-order-endpoints`

## Pull Request Guidelines

When submitting a pull request:

* keep the PR focused on a single concern
* include a clear description of what changed and why
* link related issues when applicable
* update documentation if behavior or usage changed
* add or update tests when fixing bugs or adding features
* avoid unrelated formatting-only changes unless the PR is specifically for formatting cleanup

A good pull request should make it easy to answer:

* What problem does this solve?
* What changed?
* How was it tested?
* Does it introduce breaking behavior?

## Coding Standards

Please follow the existing project style and conventions.

General expectations:

* keep code readable and maintainable
* prefer small, focused methods
* avoid unnecessary breaking API changes
* preserve backward compatibility where practical
* use meaningful naming
* avoid dead code and commented-out code
* document non-obvious behavior

For public APIs:

* keep method and type names clear and consistent
* add or update XML documentation where appropriate
* consider developer ergonomics and discoverability

## Tests

Contributions should include tests when appropriate.

Please:

* add unit tests for new logic
* add regression tests for bug fixes when possible
* ensure existing tests continue to pass
* avoid introducing flaky tests

If a change cannot be covered by an automated test, explain how it was validated in the pull request description.

## Documentation

Please update documentation when relevant, including:

* `README.md`
* XML documentation comments
* examples
* configuration notes
* migration notes for breaking or behavior-changing updates

## Security and Safety Expectations

Because this project may be used in trading-related environments, please be especially careful with changes involving:

* authentication or session handling
* TLS / SSL validation
* account identifiers
* request/response logging
* order placement, modification, or cancellation
* defaults that may be safe for localhost development but unsafe for remote use

Changes in these areas may require additional review before merge.

## Commit Messages

Clear commit messages are appreciated.

Examples:

* `Fix null handling in portfolio response parser`
* `Add unit tests for session status client`
* `Update README with gateway authentication example`

## Issue Reporting

When opening an issue for a bug, please include:

* package version
* target framework
* operating system
* reproduction steps
* expected behavior
* actual behavior
* logs or stack traces, if safe to share

Please do not include secrets, tokens, cookies, account numbers, or other sensitive information.

## Review Process

All changes are reviewed before merge.

Maintainers may ask for:

* smaller scope
* additional tests
* documentation updates
* changes to naming or API shape
* security hardening for sensitive areas

## Licensing

By submitting a contribution, you agree that your contribution may be distributed under the repository's license.

## Questions

If something is unclear, open an issue for discussion before starting a large change.

