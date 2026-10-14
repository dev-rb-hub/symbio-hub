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

## Out of scope

Concrete bank or treasury integrations. These are separate, partner-dependent adapters.
