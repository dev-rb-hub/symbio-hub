# Symbio Hub

<p align="center">
  <a href="https://github.com/dev-rb-hub/symbio-hub">
    <img src="https://repository-images.githubusercontent.com/1308285720/cab93091-5614-4e02-8286-2cd93adf0cdc" alt="Symbio Hub Social Preview" width="650" />
  </a>
</p>

[![License](https://img.shields.io/github/license/dev-rb-hub/symbio-hub)](LICENSE)
[![CI](https://img.shields.io/github/actions/workflow/status/dev-rb-hub/symbio-hub/ci.yml?branch=main)](https://github.com/dev-rb-hub/symbio-hub/actions/workflows/ci.yml)

**Symbio Hub is an open-source technical marketplace built to bridge the digital divide for regional Australian small-to-medium enterprises (SMEs).** It connects regional businesses with vetted local technology professionals, innovation-hub cohorts and emerging university graduates, so digital transformation can be delivered safely and the digital capital stays in the region.

Regional businesses deserve trusted local digital expertise, not anonymous global marketplaces. Symbio Hub combines identity verification, milestone-based escrow and recorded delivery evidence so business owners can engage experts with less delivery anxiety, and local experts can be confident they will be paid for verified work.

> **Status: working MVP, early stage.** No live customers and no live transactions yet; payments run against a sandbox. Every capability in these docs carries a maturity label (**Implemented**, **Designed**, **Planned**, **Partner-dependent**) defined in the [roadmap](docs/ROADMAP.md).

Powered by a decoupled .NET backend and a React frontend, the whole platform runs on Linux, macOS and Windows through Docker.

---

## 🔗 Quick Links

- **Use it:** [Getting Started](docs/getting-started.md) · [User Guide](docs/user-guide.md) · [API Reference](docs/api-reference.md) · [Product Video Guides](docs/product-video-guides.md)
- **Understand it:** [Roadmap](docs/ROADMAP.md) · [Architecture Overview](docs/architecture/overview.md) · [Security and Compliance Posture](docs/compliance/README.md) · [FAQ and Open Questions](docs/faq.md) · [Regional Ecosystem Vision](docs/regional-ecosystem-vision.md) · [Competitor Landscape](docs/competitor-landscape.md)
- **Contribute:** [Contributing](CONTRIBUTING.md) · [Repository Issues](https://github.com/dev-rb-hub/symbio-hub/issues)

---

## 🎯 Problem & Market Solution

* **The Challenge:** Regional SMEs lack access to trusted local digital expertise, leaving them dependent on expensive metropolitan agencies or high-risk offshore outsourcing. When delivery fails or work is sent offshore, regional economies lose both money and skills.
* **The Solution:** A secure, localised marketplace built around a three-stage, risk-mitigating approach to software delivery:

| Stage | What happens | Status |
| :--- | :--- | :--- |
| **1. Verify** | Experts and businesses register with trust profiles. Identity checks are performed by accredited partners; the platform stores verification status and evidence references only ([ADR 0003](docs/architecture/adr/0003-identity-assertion-ingestion.md)). | Implemented (sandbox onboarding); partner identity routing **Designed / Partner-dependent** |
| **2. Agree** | Scoped agreements with defined milestones and delivery expectations. | Implemented |
| **3. Deliver and settle** | Funds are secured per milestone, the expert records completion evidence, the client signs off, and only then is settlement requested. | Implemented (sandbox payments with [Pinch Payments](https://getpinch.com.au)) |

* **AI-enabled modernisation:** guided scoping and artefact vetting that inform, but never replace, human sign-off. **Planned**; today AI is used to help build the product, not as a customer-facing feature.

### For enterprise and public-sector stakeholders

Symbio Hub is designed so that a regulated organisation can substitute its own approved components (payment gateway, identity provider, hosting) behind defined interfaces, and so that its security team can inspect the code. It is built to **support** a buyer's third-party risk assessment, including APRA CPS 234 style expectations. It does not make Symbio Hub "CPS 234 compliant": those obligations sit with the regulated entity. See the [compliance posture](docs/compliance/README.md) for what is and is not in place.

Hosted editions are designed for Australian-region hosting and for isolating sensitive data and PII from public AI training, using the open zero-trust [a2a-control-plane](https://github.com/dev-rb-hub/a2a-control-plane) specification we authored. Integration with Symbio Hub is **Designed** ([ADR 0002](docs/architecture/adr/0002-agent-control-plane-boundary.md)); it is not yet enforced in the running product.

### Why it matters beyond one project

A regional project delivered by a local expert keeps wages, skills and IP in the community. Over time the model aims to turn technological displacement into a regional economic asset: a pipeline from learning, to certification, to paid local work. See the [Regional Ecosystem Vision](docs/regional-ecosystem-vision.md) and [FAQ](docs/faq.md) for the ideas and open questions, none of which are commitments.

<p align="center">
  <a href="frontend/Symbio.Frontend/src/assets/images/Symbio-hub%20flyer.png">
    <img src="frontend/Symbio.Frontend/src/assets/images/Symbio-hub%20flyer.png" alt="Symbio Hub Flyer" width="650" />
  </a>
</p>

---

## 🚀 Getting Started

### ⭐ 5-Minute Demo Start

From the repository root, run:

```powershell
docker compose up --build -d
```

Open:

- Frontend SPA: `http://localhost:5173`
- Backend Swagger: `http://localhost:5001/swagger`

The local demo runs in Development mode with seeded sample accounts and sandbox payments. For setup options (Docker, Dev Containers, Codespaces, bare metal), environment configuration, role-by-role walkthroughs and the repository layout, see [Getting Started](docs/getting-started.md), the [User Guide](docs/user-guide.md) and the [API Reference](docs/api-reference.md).

---

## 🗺️ Status and Roadmap

**Working MVP at demo grade, with sandbox payments and no live customers or transactions.** Direction is in three horizons: the open-source **Community** platform (now), **Pro and Enterprise** hosted editions, and **sovereign and public-sector** deployments. Each item is labelled Implemented, Designed, Planned or Partner-dependent.

- [Roadmap](docs/ROADMAP.md): horizons, MVP delivery status and known limitations
- [Symbio Hub Community Roadmap board](https://github.com/users/dev-rb-hub/projects/1)

Do not use the Community edition as-is with real customer data or funds.

---

## 🧩 Editions

| Edition | Audience | What it adds | Status |
| :--- | :--- | :--- | :--- |
| **Community** (this repository) | Regional SMEs, experts, students, contributors | Open-source under Apache-2.0, self-hostable, sandbox-capable payments, onboarding verification, learning and pilot use | **Implemented** |
| **Pro** (hosted) | Innovation hubs, councils, universities, larger SME cohorts | Managed hosting and support, cohort and programme tooling, regional impact reporting | **Planned**; pricing not defined |
| **Enterprise** | Financial services and other regulated organisations | Enterprise payment gateway adapters, partner-routed identity, append-only audit and signed evidence, AI governance, Australian-region hosting, supplier-assessment documentation | **Planned** / **Partner-dependent** |

All editions are built on the same core through replaceable adapters; they differ by adapters, policy and configuration, not by diverging domain code. The Community edition stays fully usable without Pro or Enterprise. See the [roadmap](docs/ROADMAP.md) and [architecture overview](docs/architecture/overview.md).

Maintainers also keep a private north-star planning board for Pro and Enterprise initiatives, stage gates and claims-versus-evidence tracking: [Product & Platform Roadmap (private)](https://github.com/users/dev-rb-hub/projects/3). It is not publicly accessible; access for prospective design partners is on request by opening a [repository issue](https://github.com/dev-rb-hub/symbio-hub/issues) (do not include sensitive details). The public view of direction is the [roadmap](docs/ROADMAP.md).

Security and compliance statements in this repository describe design intent and current status, not certification. See the [compliance posture](docs/compliance/README.md).

---

## 🛠️ Technology Stack

* **Backend API:** .NET 8 Web API, containerised with Docker
* **Data:** Azure Cosmos DB (NoSQL API) repositories alongside SQLite for local relational data
* **Frontend SPA:** React 19 (TypeScript), Vite
* **DevOps:** GitHub Actions CI, CodeQL, Dependabot, Dev Containers; Bicep templates for future Azure deployment (not deployed)

See the [repository layout](docs/getting-started.md#repository-layout) and [architecture overview](docs/architecture/overview.md).

---

## 🤝 Contributing

Review our [Contribution Guidelines](CONTRIBUTING.md) to understand branching conventions, pull request validations, and mock data parameters. Include `Closes #<Issue-Number>` in PR descriptions to auto-close roadmap stories.

---

## 📄 License

This project is licensed under the **Apache License 2.0**. See the [LICENSE](LICENSE) file for comprehensive details.

## ⚖️ Australian Consumer Law & Platform Disclaimer
Symbio Hub is an open-source software repository provided under the Apache License 2.0. 

1. Source Code: The provision of this source code is free of charge and does not constitute an act of trade or commerce under the Australian Consumer Law (ACL). No statutory consumer guarantees apply to the unhosted source code files.
2. Hosted Environments & Payments: If you are accessing a live, hosted instance of Symbio Hub or utilizing its integrated Pinch Payments features, those services are governed by separate, explicit commercial Terms of Service. The open-source maintainers accept zero liability for transactional failures, escrow disputes, or data loss occurring on downstream deployments.
3. Third-party names: Symbio Hub is an independent project. Third-party names, including Pinch Payments, are used only to identify integrations and do not imply affiliation, sponsorship or endorsement. Pitch videos and archived hackathon material describe a concept in development, not a hosted release.

---

## 💖 Sponsor This Project

Symbio Hub is an open-source platform dedicated to bridging the digital divide for regional Australian SMEs. By sponsoring this project, you help fund future pilot hosting and tooling (for example Azure Container Apps, Cosmos DB and telemetry) and help keep our regional talent ecosystem active. The project is currently pre-revenue and run by a sole founder; sponsorship funds are not yet tied to any committed infrastructure budget.

*Important Note:* Sponsorships are voluntary donations to support open-source development. They do not constitute a commercial transaction, do not create an act of "trade or commerce" under the Australian Consumer Law, and grant no commercial software warranties, service level agreements (SLAs), or priority development entitlements.

### Choose Your Sponsorship Tier 🚀

| Tier | Monthly Support | Recognition Details |
| :--- | :--- | :--- |
| **🌱 Project Supporter** | \$5 AUD/mo | • Sponsor badge displayed on your GitHub profile.<br>• Listed in our open-source `CONTRIBUTORS.md` file. |
| **🚀 Ecosystem Builder** | \$15 AUD/mo | • All benefits listed above.<br>• Your name or profile linked in the repository README. |
| **🏗 Infrastructure Supporter** | \$45 AUD/mo | • All benefits listed above.<br>• Your logo or name featured in the sponsors section below. |

<p align="center">
  <a href="https://github.com/sponsors/dev-rb-hub">
    <img src="https://img.shields.io/badge/Sponsor-dev--rb--hub-ea4aaa?logo=githubsponsors&logoColor=white" alt="Sponsor Symbio Hub on GitHub Sponsors" />
  </a>
</p>

Primary sponsorship profile: **https://github.com/sponsors/dev-rb-hub**

### 👑 Sponsors

*Thank you to everyone who supports the project:*

<!-- SPONSOR_LOGO_START -->
<p align="center">
  <a href="https://github.com/sponsors/dev-rb-hub"><strong>dev-rb-hub</strong></a>
  <br/>
  <i>Founding sponsor and project maintainer.</i>
</p>
<!-- SPONSOR_LOGO_END -->
