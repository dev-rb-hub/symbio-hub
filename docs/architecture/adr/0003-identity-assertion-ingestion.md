# ADR 0003: Identity assertion ingestion

- **Status:** Proposed
- **Date:** 2026-10-14

## Context

The platform needs trustworthy identity for experts and businesses before enabling marketplace roles. Direct integration with government digital identity systems is a regulated, accredited activity and is not assumed. Regulated customers will already operate verified KYC and AML/CTF processes.

## Decision

Add an identity assertion port in `Symbio.Core` that accepts signed identity assertions from configured trusted issuers and exposes verification status.

- The community edition uses its existing onboarding verification.
- Hosted editions can configure external issuers, for example accredited partner or customer identity providers, without changing Core.
- Role activation is gated on verification status. The platform stores status and evidence references, not raw identity documents.
- Authentication for hosted editions should use OIDC with asymmetric signing keys and enforce MFA through the identity provider.

## Consequences

- No claim of direct government digital identity integration is made. The platform is architected to consume assertions routed through accredited partners.
- Screening and verification are performed by the issuer, not by Symbio Hub.
- Trust configuration (issuers, keys, clock skew) becomes security-sensitive and must be audited.
