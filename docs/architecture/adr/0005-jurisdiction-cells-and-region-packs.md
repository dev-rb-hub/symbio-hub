# ADR 0005: Jurisdiction cells and region packs

- **Status:** Proposed
- **Date:** 2026-10-21

## Context

Symbio Hub starts in Australia, and regulated or sovereign buyers need data, keys and support access to stay in a defined jurisdiction. The same buyers, or institutions in other countries, may later want to host and own the product themselves. Without a deliberate boundary, jurisdiction-specific behaviour (payments, identity, tax, retention, templates) would leak into Core and force forks. See the [global vision](../../global-vision.md).

## Decision

Treat each deployment as an independent **jurisdiction cell**, configured by a versioned **region pack**.

- A cell owns its data stores, keys, logs, backups and model endpoints, all inside one jurisdiction. Cells share no personal or project data by default.
- A region pack is configuration, policy and adapters selected through the existing ports ([ADR 0001](0001-provider-neutral-core-and-payment-gateway.md), [ADR 0003](0003-identity-assertion-ingestion.md), [ADR 0004](0004-cryptography-key-management-and-authentication.md)): payment adapter, identity adapter, residency and retention policy-as-code, tax and currency settings, locale, and legal template set.
- Core gains neutral abstractions only (for example a jurisdiction policy and tax/invoice port). It must not reference a country, regulator, cloud vendor or payment provider.
- A cell must start and operate without contacting the maintainers. No mandatory licence check or telemetry.
- Cross-cell interaction (federation) is a separate, opt-in, minimal-metadata contract and is out of scope here.
- A region pack is released only with named owners and linked evidence for each component, including local legal review of templates.
- Deployment automation (for example infrastructure-as-code per cloud) lives in `infrastructure/` and is isolated from backend and frontend code.

## Consequences

- New jurisdictions are added by writing a pack and adapters, not by diverging domain code.
- Residency and support-access rules are testable as policy, not just documented.
- Region packs create a versioning and compatibility testing burden; a shared adapter contract suite is required.
- Some features (for example cross-border matching) are intentionally harder to build, in exchange for sovereignty.
- Adopting this decision makes no compliance claim for any jurisdiction.

## Out of scope

Concrete second-jurisdiction adapters, federation protocol design, and commercial or licensing terms for regional operators.
