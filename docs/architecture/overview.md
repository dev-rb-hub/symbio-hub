# Architecture Overview

High-level view of how Symbio Hub is structured and where it is intended to grow. For direction and maturity labels see the [roadmap](../ROADMAP.md).

## Current structure

- `backend/Symbio.API` – ASP.NET Core host, controllers, hubs, background workers.
- `backend/Symbio.Core` – domain models, contracts and services.
- `backend/Symbio.Infrastructure` – repositories and external integrations.
- `frontend/Symbio.Frontend` – React SPA; it must not depend on backend internals.
- `infrastructure/bicep` – infrastructure as code.

## Design principles

1. **Provider-neutral core.** `Symbio.Core` holds domain concepts only. It must not reference a payment provider, identity provider, cloud vendor or agent transport.
2. **Ports and adapters.** External dependencies sit behind interfaces in Core, with adapters in Infrastructure selected by configuration.
3. **Edition by composition.** The community and hosted editions differ by adapters, policy and configuration, not by diverging domain code.
4. **Strict agent separation.** Application logic lives here. Zero-trust agent messaging lives in [a2a-control-plane](https://github.com/dev-rb-hub/a2a-control-plane). They meet only through a versioned contract behind a Core port ([ADR 0002](adr/0002-agent-control-plane-boundary.md)).
5. **Evidence over assertion.** Security and compliance statements must link to a control, test or document, and use the maturity labels from the roadmap.
6. **Human accountability.** Automated checks inform decisions; they do not release funds on their own.

## Target ports

| Port | Responsibility | Status |
|---|---|---|
| Escrow payment gateway | Open escrow, hold funds, release a milestone, refund, query status | Designed ([ADR 0001](adr/0001-provider-neutral-core-and-payment-gateway.md)); today the Core contracts are provider-specific |
| Identity assertion | Accept and validate verified identity assertions, expose verification status | Designed ([ADR 0003](adr/0003-identity-assertion-ingestion.md)) |
| Agent orchestration | Submit AI tasks, receive results, no transport types in Core | Designed ([ADR 0002](adr/0002-agent-control-plane-boundary.md)) |
| Artefact vetting | Automated checks on milestone artefacts, returns an explainable verdict | Planned |
| Audit log | Append-only record of milestone and escrow transitions | Planned |

## Milestone lifecycle (provider-neutral)

`Draft → PreApproved → FundsSecured → EvidenceSubmitted → ClientSignedOff → SettlementInitiated → SettlementComplete`, with `Disputed` and `Refunded` branches. The core owns state transitions; adapters report facts such as "funds held" or "settled". Optional automated vetting adds a gate before client sign-off.

## Boundaries to keep

- Do not leak backend data structures or schemas into frontend code or public docs.
- Do not add provider SDK types to Core.
- Do not let agents access application databases or payment ports directly.
