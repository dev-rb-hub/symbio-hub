# ADR 0001: Provider-neutral core and payment gateway port

- **Status:** Proposed
- **Date:** 2026-10-14

## Context

Escrow behaviour is currently expressed through provider-specific interfaces and models (for example `IPinchDebitService`, `IPinchMerchantService`, `IPinchOnboardingGateway`, `IRecurringBillingService`, `IAccountingInvoicingService`). Controllers, background workers and webhook handlers depend on them directly, and configuration is provider-shaped. Replacing the payment provider would touch the API layer and the domain.

## Decision

Introduce a provider-neutral `IEscrowPaymentGateway` port in `Symbio.Core`, with neutral request and result models, a capabilities descriptor and a runtime-mode report.

- Operations: open escrow, hold funds, release a milestone, refund, query status.
- Providers become adapters in `Symbio.Infrastructure`, selected by configuration (for example `Payments:Provider`). The existing provider is the first adapter and keeps current behaviour.
- Provider webhooks and signature validation stay inside the adapter and are mapped to a neutral settlement event.
- Controllers, workers and the milestone state machine depend only on the port.
- A shared contract test suite must pass for every adapter, including a simulator for bank-style settlement.

## Consequences

- Alternative gateways can be added without changing Core, controllers or workers.
- Existing tests continue to pass with the current provider as the default.
- Some provider-specific endpoints and models will be renamed or moved.
- Provider-specific behaviours that do not fit the port must be exposed as declared capabilities, not leaked into Core.

## Provider taxonomy (for scoping adapters)

Candidate providers fall into three classes. Only the first two are payment-gateway adapters; the third is a different concern and must not be filed under this port.

- **Escrow-native** (for example Pinch, Zai/Assembly): support hold, milestone release, refund and marketplace split directly. These map cleanly onto the port and are the near-term Australian targets.
- **Acquiring/checkout-only** (for example Amazon Payment Services, Apple Pay, Google Pay): collect card/wallet payments but have no native escrow or sub-merchant split. They are allowed only with a capability descriptor declaring `SupportsNativeEscrow = false`, paired with a complementary hold mechanism. Availability is often region-bound (for example Amazon Payment Services is MENA-only), so they are selected per region pack ([ADR 0005](0005-jurisdiction-cells-and-region-packs.md)), not globally.
- **Not a payment gateway** (AWS, Azure and other cloud vendors): provide hosting, key management and AI runtimes, not escrow or payouts. These belong to key management ([ADR 0004](0004-cryptography-key-management-and-authentication.md)), the agent/AI runtime boundary ([ADR 0002](0002-agent-control-plane-boundary.md)) and deployment automation in `infrastructure/`, never behind this port.

Concrete external adapters (escrow-native or acquiring-only) are partner-dependent and gated on merchant onboarding, KYC and settlement testing; crypto-settlement providers additionally carry AML, custody and regulatory burden and are out of near-term scope.

## Out of scope

Concrete bank or treasury integrations. These are separate, partner-dependent adapters.
