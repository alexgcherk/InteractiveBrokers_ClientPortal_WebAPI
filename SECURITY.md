# Security Policy

Thank you for helping keep `InteractiveBrokers_ClientPortal_WebAPI` and its users secure.

This project interacts with broker-related APIs and may be used in environments involving authentication, account data, market data, and order placement. Security issues are taken seriously.

## Supported Versions

Security updates are provided for the latest released version and the current `main` branch.

| Version | Supported |
| ------- | --------- |
| Latest release | ✅ |
| `main` branch | ✅ |
| Older releases | ❌ |

## Reporting a Vulnerability

Please **do not open a public GitHub issue** for suspected security vulnerabilities.

Instead, report vulnerabilities privately using one of the following methods:

- **Preferred:** GitHub Private Vulnerability Reporting / Security Advisories for this repository
- **Alternative:** Contact the maintainer directly through the contact method listed in the repository profile or README

When reporting, please include as much of the following as possible:

- A clear description of the issue
- Steps to reproduce
- Proof-of-concept code, request, or configuration
- Affected version(s) or commit(s)
- Impact assessment
- Any suggested remediation, if available

## What to Report

Examples of security issues include, but are not limited to:

- Authentication or session-handling flaws
- TLS / certificate validation bypass issues
- Credential, token, or secret exposure
- Authorization issues
- Unsafe defaults that could expose trading/account activity
- Request signing, cookie, or header handling weaknesses
- Dependency vulnerabilities with practical impact
- Remote code execution, injection, SSRF, deserialization, or similar issues
- Sensitive data leakage in logs, exceptions, or diagnostics

## Project-Specific Notes

This library may be used to communicate with Interactive Brokers Client Portal / Gateway environments.

Please pay particular attention to vulnerabilities involving:

- Session reuse or session leakage
- Account identifier exposure
- Order placement, modification, or cancellation behavior
- TLS certificate validation and any SSL bypass options
- Unsafe configuration that could be acceptable for localhost development but dangerous in remote deployments

If a finding involves insecure use of `IgnoreSslErrors`, loopback certificate handling, or non-localhost deployments, please describe the exact host/network scenario in your report.

## Response Process

The maintainer will aim to:

- Acknowledge receipt of the report within **7 days**
- Perform an initial triage within **14 days**
- Provide periodic status updates during investigation
- Coordinate disclosure after a fix or mitigation is available

Please note that response times may vary depending on complexity and maintainer availability.

## Disclosure Policy

Please allow reasonable time for investigation and remediation before making any vulnerability public.

After a fix is available, the maintainer may:

- Publish a security advisory
- Credit the reporter, if desired
- Release a patched version
- Document any required mitigation steps for users

## Safe Harbor

If you act in good faith to identify and report a security issue, and:

- Avoid privacy violations, data destruction, or service disruption
- Do not access, modify, or exfiltrate data beyond what is necessary to demonstrate the issue
- Do not publicly disclose the issue before coordinated remediation

then your research will be treated as authorized and appreciated.

## Security Best Practices for Users

Users of this library should:

- Prefer the latest released version
- Avoid disabling TLS validation except for controlled localhost-only development scenarios
- Never use insecure SSL/TLS settings in production or over untrusted networks
- Store credentials and secrets securely
- Review logs to ensure sensitive information is not emitted
- Validate all deployment and gateway configuration carefully before enabling trading actions

## Non-Security Bugs

For general bugs, feature requests, and usability issues, please use normal GitHub Issues.
