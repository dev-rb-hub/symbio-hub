# Global Vision: Australia First, Built to Be Replicated

How Symbio Hub could grow from an Australian regional marketplace into a pattern that other regions and enterprises can host, own and support under their own laws. This page states **hypotheses and intent**, not commitments, partnerships or advice. No organisation, marketplace or regulator named here has agreed to anything. Maturity labels (**Implemented**, **Designed**, **Planned**, **Partner-dependent**) are defined in the [roadmap](ROADMAP.md).

## The position in one line

**Local by design. Portable by architecture. Australia first.**

Symbio Hub is not trying to become another anonymous global marketplace. The thesis is the opposite: many **sovereign regional marketplaces**, each keeping data, wages and skills in its own region, all built from the same open core. Australia is the first region and the proof point; the architecture is what lets the model travel.

## Why this is credible now

| Claim | Basis today |
| :--- | :--- |
| The core is provider-neutral | **Designed**: [ADR 0001](architecture/adr/0001-provider-neutral-core-and-payment-gateway.md), [architecture overview](architecture/overview.md). Today the Core contracts are still provider-specific, and this is stated openly. |
| Editions differ by adapters and policy, not forked code | **Designed** principle; the Community edition is **Implemented** |
| Identity, payments, hosting and AI runtimes are replaceable | **Designed** (ADRs 0001 to 0004); only the sandbox payment adapter exists |
| Region and jurisdiction rules can be packaged | **Designed** as the [region pack](architecture/adr/0005-jurisdiction-cells-and-region-packs.md) concept, not built |
| Anyone can inspect and self-host it | **Implemented** (Apache-2.0) |

Nothing on this page is a claim of market traction. There are no live customers or transactions yet.

## Deployment and ownership models

One codebase, several ways to run it. All are **Planned** except self-hosting the Community edition.

| Model | Who hosts | Who supports | Where data lives | Fit | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Self-hosted Community** | The operator | Community and the operator | Operator's choice | Universities, hubs, contributors, pilots | **Implemented** |
| **Symbio-operated regional cell** | Symbio Hub, in one jurisdiction | Symbio Hub | Stays in that jurisdiction | Pro customers: hubs, councils, cohorts | **Planned** |
| **Customer-owned tenancy ("own-hosted")** | The customer, in its own cloud account or data centre | The customer, with a vendor support agreement | Customer-controlled | Enterprises and regulated buyers that must own the runtime | **Planned** |
| **Partner-operated regional host** | A licensed local operator | The partner, with escalation to the core maintainers | In the partner's jurisdiction | New countries without a Symbio entity | **Planned**, **Partner-dependent** |
| **Sovereign or government environment** | Government or an accredited provider | Cleared local personnel | Inside the protected environment | Public sector, critical infrastructure | **Planned** (Horizon 3) |

Design rules that apply to every model:

- **Data stays in its cell.** Personal and project data never leaves the jurisdiction it was created in unless the user explicitly requests a cross-border engagement.
- **No central dependency for operation.** A cell, including an own-hosted one, must keep running if the maintainers are unreachable. Licence checks or phone-home behaviour must not be required for core functions.
- **Customer-chosen components.** Payment gateway, identity provider, key management and AI runtime are the customer's approved choices, behind the existing ports.
- **Support is tiered and honest.** No 24/7 or SLA claim is made until there is staffing and a signed commercial agreement. Follow-the-sun support is only realistic through partners.

## Region packs: how a new jurisdiction is added

A **region pack** is the unit of replication ([ADR 0005](architecture/adr/0005-jurisdiction-cells-and-region-packs.md), **Proposed**). It is configuration and adapters, not a code fork.

| Component | Examples | Owner |
| :--- | :--- | :--- |
| Payment and escrow adapter | A licensed marketplace-payments provider or bank in that country | Partner |
| Identity assertion adapter | Accredited identity and KYC provider | Partner |
| Hosting and residency policy | Approved regions, policy-as-code, key custody | Operator |
| Model endpoint policy | Approved, zero-retention, in-region AI endpoints | Operator |
| Legal templates | Lawyer-reviewed statement of work, terms, privacy notice, tax invoice | Local counsel |
| Tax, currency and locale | GST, VAT or sales tax handling, invoice format, language | Local advisers |
| Control mapping | Mapping to local supplier-assessment expectations | Maintainers with local assessors |

A region pack is only released when each row has a named owner and linked evidence.

## Global distribution channels (candidates, none live)

"Marketplace" here has two meanings, and we keep them apart:

1. **Distribution marketplaces**: where enterprises discover, procure and deploy an own-hosted edition.
2. **The talent marketplace itself**: stays regional. Cross-region matching is optional federation, below.

Candidate distribution channels, to be tested in order of buyer demand:

| Channel | What it would offer | Status |
| :--- | :--- | :--- |
| Public cloud marketplaces (container, managed application or SaaS listings, private offers) | Procurement through a buyer's existing cloud commitment | **Planned**, exploratory; no listing exists |
| Signed container images, Helm charts and infrastructure-as-code per cloud | Repeatable own-hosted installs; today only Azure Bicep templates exist and they are not deployed | **Planned** (supports INI-08) |
| Government and public-sector procurement panels in each country | Access to sovereign and public-sector buyers | **Planned**, **Partner-dependent**; each has its own eligibility and security prerequisites |
| Regional host partners | Local operation, support and language | **Partner-dependent** |
| Open-source distribution (GitHub, package registries) | Developer and institution adoption | **Implemented** (source only) |

Listing on any marketplace or panel generally requires security evidence, supplier documentation and sometimes assessments that Symbio Hub does not yet hold (see the [compliance posture](compliance/README.md)). Listing is a consequence of evidence, not a substitute for it.

## Optional federation across regions (**Planned**, exploratory)

- Each regional cell remains sovereign and operates alone.
- Federation is opt-in per cell and per user. A cross-border engagement is created only on explicit request from both parties.
- Only the minimum needed to match and contract is shared, such as skill tags and verified-status references. Identity documents, payment data and project detail do not cross a cell boundary by default.
- Cross-border payments, tax and contractor rules are handled by the licensed partners in each jurisdiction, not by the platform.

## Legal considerations by jurisdiction

These are the topics counsel and accredited advisers must review **before** any hosted launch in a jurisdiction. This is a planning checklist, not legal advice, and it is not a statement that any obligation has been met. Specific laws change; confirm with local counsel.

### Australia (first cell)

| Topic | Why it matters |
| :--- | :--- |
| Privacy Act 1988 and Australian Privacy Principles, including cross-border disclosure (APP 8) and the Notifiable Data Breaches scheme | Governs personal information in hosted editions and any offshore AI or support access |
| APRA CPS 234 and CPS 230 | Obligations sit with regulated buyers; the platform supports their third-party assessment ([compliance posture](compliance/README.md)) |
| Australian Consumer Law | Hosted terms, consumer guarantees and disclaimers (see the README note) |
| Payments and financial services licensing; AML/CTF obligations | The platform must not hold funds or provide a financial service itself; licensed partners do |
| Government security frameworks (for example IRAP, the Information Security Manual and Hosting Certification Framework) | Prerequisites for public-sector and sovereign deployments |
| Security of Critical Infrastructure obligations | Relevant to some enterprise and infrastructure buyers |
| Workplace-relations rules on contractors and digital labour platforms | A marketplace must avoid structures that create sham contracting or employee-like arrangements by accident |

### Other candidate jurisdictions (illustrative, unvalidated)

| Region | Topics to review |
| :--- | :--- |
| European Union | GDPR and international transfer rules; DORA for financial-sector buyers; NIS2; the AI Act for AI-assisted scoping and vetting; Data Act; platform-work rules |
| United Kingdom | UK GDPR; regulator outsourcing and critical third-party expectations for financial buyers; FCA and PRA expectations |
| United States | State privacy laws; SOC 2 as a market expectation; FedRAMP for federal buyers; state-level money transmission rules for payment flows; marketplace-facilitator tax |
| New Zealand and Singapore | Privacy Act 2020; PDPA and financial-sector outsourcing and technology-risk guidance; likely closest to Australian norms, to be validated |
| Canada, Japan, India and others | PIPEDA, APPI and the DPDP Act respectively; data-localisation requirements in some markets |

### Cross-cutting topics

- **Licence and brand.** Apache-2.0 grants no trademark rights. Before third parties host or resell, Symbio Hub needs a published trademark and naming policy so that forks and regional hosts cannot imply endorsement. **Planned.**
- **IP chain of title.** For investor and enterprise diligence, contributions should come with a documented contributor sign-off (for example a Developer Certificate of Origin or contributor licence agreement). None is in place today. **Planned.**
- **Corporate entity and tax.** Which entity contracts, bills and pays tax in each cell; GST, VAT and sales tax on platform and subscription fees.
- **Payments.** A licensed partner holds funds in every jurisdiction. No stored-value credits ([roadmap guardrails](ROADMAP.md#product-guardrails-for-new-ideas)).
- **Sanctions and export controls.** Screening for cross-border engagements and for the distribution of cryptographic software.
- **AI regulation.** Human sign-off remains mandatory; model endpoints must meet each region's retention and residency rules.
- **Support access.** Any cross-border support access to a customer's production data is itself a transfer and needs a policy, approval and logging.

## Stage gates

Each gate needs linked evidence before the next claim is made. No dates are implied.

| Gate | What must be true | Evidence |
| :--- | :--- | :--- |
| **0. Prove the model (now)** | Regional pilot run on sandbox payments; lessons published, including what failed | Pilot write-up |
| **1. Australian hosted Pro** | Hardened image, SBOM and signed artefacts per release; independent penetration test; Australian-region pinning; terms and privacy notice reviewed by counsel | Compliance posture updated with links |
| **2. Australian Enterprise and own-hosted** | Payment and identity partners contracted; audit trail and signed evidence built; supplier-assessment pack; design-partner security review | Design-partner feedback, assessment pack |
| **3. First second-jurisdiction cell** | Region pack complete with named partners and local counsel sign-off; one design partner in that country | Region pack and counsel memo |
| **4. Distribution and regional operators** | Marketplace listings or panels pursued only where Gates 1 to 3 evidence satisfies their prerequisites; operator partner agreement template | Listing prerequisites checklist |

Pro and Enterprise pricing is not defined. Commercial model options (hosted subscription, enterprise licence and support, partner revenue share) are hypotheses to test with design partners.

## Investor lens: honest answers

| Question | Current answer |
| :--- | :--- |
| What is the market thesis? | Regional SMEs, institutions and regulated enterprises want trusted local delivery with evidence, and sovereign control of where software runs. Unvalidated; to be tested in a pilot. |
| What is the defensibility? | Settlement tied to recorded delivery evidence, replaceable adapters that regulated buyers can approve, an open and inspectable core, and a reusable region-pack pattern ([competitor landscape](competitor-landscape.md)). Positioning, not proof. |
| What is the traction? | None. Working MVP at demo grade, sandbox payments, no live customers. |
| What are the main risks? | Marketplace liquidity; regulatory and licensing dependence on partners; sole-founder capacity; open-core funding; security assurance still to be earned. |
| What would change the plan? | Pilot data that contradicts a positioning claim, or a design partner whose requirements differ from the ports-and-adapters model. |
| Where do I discuss it? | Open a [repository issue](https://github.com/dev-rb-hub/symbio-hub/issues) without sensitive details to request access to the private north-star board. |

## What we are careful not to claim

- That Symbio Hub is available, or has customers, outside Australia, or at all in production.
- That any cloud marketplace, government panel, regulator, bank or technology vendor lists, approves, partners with or endorses Symbio Hub.
- That the platform is certified, assessed or compliant with any law or standard in any jurisdiction. See the [compliance posture](compliance/README.md).
- That global replication is cheap. Each jurisdiction needs partners, counsel and evidence, and many will not be worth entering.
- That regional economic outcomes are proven. See the [Regional Ecosystem Vision](regional-ecosystem-vision.md).

See also the [roadmap](ROADMAP.md) (Horizon 4 and INI-13 to INI-15), the [architecture overview](architecture/overview.md) and the [FAQ](faq.md).
