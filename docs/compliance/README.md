# Security and Compliance Posture

This page describes how Symbio Hub is designed to **support** the security, third-party risk and data residency obligations of organisations that adopt it. It is not a statement of certification or regulatory compliance.

Obligations such as APRA CPS 234 apply to the regulated entity and its whole operating environment. A software product can support a buyer's assessment but cannot itself be "compliant" with them.

## Status summary

| Area | Status |
|---|---|
| Role-based access with gated admin role | Implemented |
| Webhook signature validation | Implemented |
| Containerised build and local Dev Container | Implemented |
| Minimal, non-root, multi-stage container image | Planned |
| Dependency updates and alerts, secret scanning with push protection, CodeQL code scanning | Implemented (controls enabled; findings are tracked, not zero) |
| SBOM | Implemented (exported from the GitHub dependency graph; not yet generated per release) |
| Container image scanning, signed artefacts and build provenance | Planned |
| Append-only audit trail and signed completion evidence | Planned |
| Australian-region pinning and policy-as-code for hosted editions | Planned |
| Private networking, managed identity and secret management | Planned |
| Identity assertions from accredited partners | Designed |
| Zero-trust agent messaging via the control-plane specification | Designed (separate repository) |
| Independent penetration test | Planned |
| SOC 2 Type II, ISO 27001, IRAP assessment | Not held |

Labels follow the [roadmap](../ROADMAP.md).

Evidence for the repository controls: [dependabot.yml](../../.github/dependabot.yml), [ci.yml](../../.github/workflows/ci.yml) and the repository's GitHub security settings (code scanning default setup, secret scanning, push protection, private vulnerability reporting). "Implemented" here means the control is enabled, not that no findings are open.

## Supporting a buyer's CPS 234 assessment

| Theme | How the platform is designed to support it | Status |
|---|---|---|
| Information asset identification and classification | Data classification and a PII inventory | Planned |
| Controls commensurate with threats | Threat model, secure SDLC, automated scanning | Planned |
| Third-party assurance | Architecture overview, SBOM, policies, test summaries | Planned |
| Incident management | Audit trail, alerting hooks, runbooks | Planned |
| Control testing | Automated security tests, periodic penetration testing | Planned |
| Audit access | Exportable audit records | Planned |

## ASD Essential Eight

Hosted editions target the following strategies, with a published self-assessment and explicit gaps:

- Application control and patching applications and operating systems, through signed images and automated updates.
- Restricting administrative privileges, through just-in-time administration and the existing gated admin role.
- Multi-factor authentication, enforced through the identity provider.
- Regular backups, stored in Australian regions and restore-tested.

## Data residency

For hosted editions the design intent is that data, backups, logs and model endpoints remain in Australian regions, enforced with policy-as-code. The community edition is self-hosted and leaves residency decisions to the operator.

## Payments and identity

- Card data is out of scope: the platform does not store card numbers and relies on provider or bank tokenisation.
- KYC, AML/CTF screening and digital identity are performed by accredited partners. The platform stores status and evidence references only. See [ADR 0003](../architecture/adr/0003-identity-assertion-ingestion.md).

## AI use

Automated scoping and vetting are advisory. A human signs off milestones, automated checks produce explainable verdicts, and hosted editions are intended to use zero-retention, Australian-hosted model endpoints.

## Reporting a security issue

Please report suspected vulnerabilities privately rather than through public issues. See [SECURITY.md](../../SECURITY.md).
