# FAQ and Open Questions

Plain answers to common questions, and the problems we are still working out. Maturity labels (**Implemented**, **Designed**, **Planned**, **Partner-dependent**) are defined in the [roadmap](ROADMAP.md). Nothing here is a commitment, a partnership claim or a statement of certification.

## About the product

**What is Symbio Hub?**
An open-source technical marketplace connecting regional Australian SMEs with vetted local technology professionals, innovation-hub cohorts and emerging graduates. See the [README](../README.md).

**Is it live? Can I use it with real money today?**
No. It is a working MVP. Payments run against a sandbox payment provider and no real funds move. A regional pilot is **Planned**.

**How does escrow work?**
Funds are secured per milestone. The expert records completion evidence, the client signs off, and only then is settlement requested. Automated checks inform the decision; they never release funds on their own. See the [architecture overview](architecture/overview.md).

**Who holds the money?**
A regulated payment provider, not the open-source project. Today that is a sandbox-capable community provider. Enterprise editions are designed to use a buyer's own approved gateway through a replaceable port ([ADR 0001](architecture/adr/0001-provider-neutral-core-and-payment-gateway.md)).

**Does it use AI?**
AI is used to help build and maintain the product, under human review. There are no customer-facing AI features today. Customer-facing guided scoping and artefact vetting are **Planned**, with human sign-off required.

**What AI work is planned?**
All items below are **Planned** or exploratory. None is built, and none implies a partnership or endorsement by any vendor named.

- **Agentic software factory:** setup and project templates for AI-assisted delivery, with humans accountable for review and sign-off.
- **AI assistance in Pro and Enterprise editions:** workloads that help both SMEs (scoping, milestone planning) and professionals (delivery, evidence preparation) on projects.
- **AI security and compliance guardrails:** agent checks built into the delivery process and product, then extended to customers, aligned with the zero-trust [a2a-control-plane](https://github.com/dev-rb-hub/a2a-control-plane) specification ([ADR 0002](architecture/adr/0002-agent-control-plane-boundary.md)). These support a buyer's assessment; they are not a certification.
- **AI learning and certification:** AI courses and learning modules in the Community edition, and targeted programs for enterprise or institutional cohorts. See the [Regional Ecosystem Vision](regional-ecosystem-vision.md).
- **Scalable architecture options:** containerised microservices and event streaming (for example Kafka) as options for scaling and for replacing hosting components, behind the existing ports-and-adapters design ([architecture overview](architecture/overview.md)).
- **Modern hosted AI runtimes and deployment options:** adapters for managed agent and model platforms such as AWS Bedrock, Claude Managed Agents, Azure AI Foundry and NVIDIA sandbox runtimes, so enterprises can use their own approved runtime. Deployment and hosting options are intended to include self-hosted containers, managed cloud, and sovereign or on-premises environments. Selection depends on each buyer's requirements, including Australian-region availability and data-retention terms.
- **Enterprise modernisation:** supporting global modernisation initiatives where an organisation wants to adopt these patterns, delivered through partners as needed (**Partner-dependent**).

**Is the code free to use?**
Yes. The Community edition is Apache-2.0. See the licence and the Australian Consumer Law note in the [README](../README.md).

## Security, compliance and data

**Is Symbio Hub CPS 234 compliant, or certified?**
No. CPS 234 applies to the regulated entity, not to software. Symbio Hub is designed to **support** a buyer's third-party risk assessment. We hold no SOC 2, ISO 27001 or IRAP assessment. See the [compliance posture](compliance/README.md).

**Is data stored in Australia?**
Hosted editions are designed for Australian-region hosting enforced by policy-as-code (**Planned**). The self-hosted Community edition leaves residency to the operator.

**Can our data end up in public AI training sets?**
Hosted editions are designed to use zero-retention, Australian-hosted model endpoints and tenant isolation, with agent communication through the open [a2a-control-plane](https://github.com/dev-rb-hub/a2a-control-plane) specification. This is intended to mitigate exfiltration risk, not eliminate it, and it is not yet an operating control.

**How is identity verified?**
Verification is designed to be performed by accredited partners; the platform stores status and evidence references only ([ADR 0003](architecture/adr/0003-identity-assertion-ingestion.md)). **Designed / Partner-dependent.**

## Editions and roadmap

**What are Pro and Enterprise?**
Hosted editions built on the same core. Pro targets hubs, councils and universities; Enterprise targets regulated organisations. Both are **Planned** and the Community edition remains fully usable without them. See [Editions](../README.md#-editions).

**Where can I see the detailed plan?**
The public [roadmap](ROADMAP.md) and [community board](https://github.com/users/dev-rb-hub/projects/1). A private north-star board exists for Pro and Enterprise planning; it is not publicly accessible, and access can be requested by opening a repository issue.

**How is Symbio Hub different from Upwork, Fiverr or Airtasker?**
See the [competitor landscape](competitor-landscape.md). In short: regional focus, trust-first onboarding, and settlement tied to recorded delivery evidence.

## Open questions we want to solve

These are problems, not promises. Feedback and issues are welcome.

1. **Regional matching.** How do we define "local" fairly (postcode, travel radius, remote-but-regional) without shrinking the talent pool too far?
2. **Scoping for non-technical owners.** How can an SME turn a business goal into a good scope and milestones, and who reviews it?
3. **Evidence quality.** What counts as sufficient delivery evidence for different work types (code, configuration, training, documentation)?
4. **Disputes.** What is a proportionate, human-led dispute path that keeps funds held fairly and quickly?
5. **Graduate pathways.** How do students and early-career experts build a verified track record safely, with mentoring and supervision?
6. **Bank and enterprise fit.** Which supplier-assessment questions do regulated buyers actually ask, and what evidence should we publish to answer them?
7. **Impact measurement.** How do we measure local jobs retained, skills built and spend kept in region without collecting more personal data than needed?
8. **Sustainable funding.** How do we fund the open-source core while keeping the Community edition free?
9. **Responsible AI.** What evaluation and change control should apply before any AI feature influences a decision?

See also the [Regional Ecosystem Vision](regional-ecosystem-vision.md).
