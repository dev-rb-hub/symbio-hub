# ADR 0002: Agent control-plane boundary

- **Status:** Proposed
- **Date:** 2026-10-14

## Context

Symbio Hub plans AI-assisted features such as project scoping and artefact vetting. Multi-agent messaging needs strong identity, encrypted transport and network segmentation, which is specified separately in [a2a-control-plane](https://github.com/dev-rb-hub/a2a-control-plane).

## Decision

Keep application logic and agent messaging in separate repositories, release cycles and trust zones.

- Core defines an `IAgentOrchestrationPort` with task and result models that contain no transport, identity-framework or messaging types.
- An adapter in `Symbio.Infrastructure` talks to the control plane through versioned Protobuf contracts owned by the control-plane repository and consumed as a pinned dependency.
- The community edition ships a no-op or in-process implementation and runs without the control plane.
- Worker agents never access application databases or payment ports. Results return through the port.
- CI includes an architecture test that fails if Core references agent transport types.
- Ownership of each repository is separated through CODEOWNERS.

## Consequences

- Either side can change independently if the contract stays compatible.
- The control plane's security properties (short-lived workload identity, mutual TLS, network policy) are enforced by deployment, not by application code.
- AI features degrade gracefully when the control plane is absent.
- Contract versioning and compatibility testing become required work.
