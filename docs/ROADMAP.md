# Symbio Hub Roadmap

This roadmap describes direction, not commitments. Delivery status and live work items are tracked in the [Symbio Hub - Community Roadmap](https://github.com/users/dev-rb-hub/projects/1) and the [repository issues](https://github.com/dev-rb-hub/symbio-hub/issues).

Maintainers also keep a private north-star board for Pro and Enterprise planning, stage gates and claims-versus-evidence tracking: [Product & Platform Roadmap (private)](https://github.com/users/dev-rb-hub/projects/3). It is not publicly accessible; request access by opening a repository issue. Questions we are working through are in the [FAQ](faq.md), and longer-term ideas are in the [Regional Ecosystem Vision](regional-ecosystem-vision.md).

Maturity labels used across the docs:

- **Implemented** – working in this repository today.
- **Designed** – architecture agreed, not yet built.
- **Planned** – on the roadmap, not started.
- **Partner-dependent** – requires an accredited or regulated third party.

## Horizon 1: Community platform (current)

Open-source marketplace connecting regional SMEs with vetted local technical talent.

- **Implemented:** role-based flows, project posting, talent discovery, agreements, expert workbench, completion evidence, milestone escrow using a sandbox-capable payment provider, admin operations.
- **Designed:** provider-neutral payment gateway port so the community payment provider is replaceable ([ADR 0001](architecture/adr/0001-provider-neutral-core-and-payment-gateway.md)).
- **Implemented:** repository security controls: Dependabot updates and alerts, secret scanning with push protection, CodeQL code scanning, private vulnerability reporting. Findings are tracked and triaged, not zero.
- **Planned:** documentation baseline, hardened container image, image scanning, per-release SBOM and signed artefacts, regional pilot with local innovation hubs and universities, and supervised student and graduate cohorts using the Community edition as a learning platform (see the [Regional Ecosystem Vision](regional-ecosystem-vision.md)).

## Horizon 2: Regulated-enterprise editions

Hosted editions for organisations with stricter security, identity and payments requirements. The community edition remains fully usable without these.

- **Designed:** identity assertion ingestion from accredited partners ([ADR 0003](architecture/adr/0003-identity-assertion-ingestion.md)); agent orchestration port with an open zero-trust control-plane boundary ([ADR 0002](architecture/adr/0002-agent-control-plane-boundary.md)).
- **Planned:**
  - Enterprise payment gateway adapters behind the payment port.
  - Append-only audit trail and signed completion evidence.
  - AI-assisted scoping and artefact vetting with human sign-off, an agentic software factory with templates, and agent security and compliance guardrails (see the [FAQ](faq.md)).
  - Scalable containerised microservice and event-streaming options, adapters for managed agent and AI runtimes (for example AWS Bedrock, Claude Managed Agents, Azure AI Foundry, NVIDIA sandbox runtimes), and self-hosted, managed-cloud and sovereign deployment options.
  - AI learning modules and certification pathways for the Community edition and targeted enterprise programs.
  - Sovereign Australian hosting, policy-as-code and tenant isolation.
  - Supplier-assessment documentation (see [compliance](compliance/README.md)).
- **Partner-dependent:** KYC/AML screening, digital identity assertions, bank or treasury integrations.

## Horizon 3: Sovereign and public-sector deployments

- **Planned:** deployment onto protected or government-hosted environments, federated identity, regional impact reporting, and optional allocation tracking for sponsored compute.

## Cross-cutting principles

See the [architecture overview](architecture/overview.md): a provider-neutral core, ports and adapters, strict separation from the agent control plane, and evidence-backed claims.
