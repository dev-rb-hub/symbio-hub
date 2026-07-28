# Symbio Hub

<p align="center">
  <a href="https://github.com/dev-rb-hub/symbio-hub">
    <img src="https://repository-images.githubusercontent.com/1308285720/708960c5-1563-422e-9d4f-4f90c466e189" alt="Symbio Hub Social Preview" width="650" />
  </a>
</p>

[![License](https://shields.io)](LICENSE)
[![GitHub Workflow Status](https://shields.io)](https://github.com/dev-rb-hub/symbio-hub/actions/workflows/ci.yml)

Regional businesses deserve trusted local digital expertise—not anonymous global marketplaces. 
Symbio Hub provides a secure marketplace where verified local professionals collaborate with confidence through protected payments and AI-enabled modernization

Symbio Hub is a cross-platform, containerised open-source platform connecting regional SMEs with vetted technical talent. Powered by a decoupled .NET Core backend and a React frontend, the entire ecosystem runs seamlessly across Linux, macOS, and Windows via Docker virtualization.

---

## 🎯 Problem & Market Solution

* **The Challenge:** Regional SMEs lack access to trusted local digital expertise, leaving them dependent on expensive metropolitan agencies or high-risk offshore outsourcing.
* **The Solution:** A secure, localized marketplace featuring:
    * **Verified Trust Profiles:** KYC-enabled expert registration profiles.
    * **Risk-Free Escrow:** Milestone-based payments powered by [Pinch Payments](https://getpinch.com.au).
    * **AI Modernization:** Tailored deployment tracks for local AI and automation workflows.

Product Pitch - see [Hackathon Pitch](docs/pinch-pitch.md).
For competitor context and market positioning, see [Competitor Landscape](docs/competitor-landscape.md).

---

## 🏗️ Cross-Platform Architecture & Virtualization

To eliminate "it works on my machine" bottlenecks and support open-source developers on any operating system, Symbio Hub enforces containerisation at every stage:

* **Production Containerisation:** The .NET backend is containerised via a multi-stage Linux Alpine Dockerfile, allowing deployment to cloud environments like Azure Container Apps (ACA), AWS ECS, or native Kubernetes.
* **Standardised Development (Dev Containers):** Developers can launch a fully isolated workspace using VS Code Dev Containers. This automatically bundles the .NET SDK, Node.js runtime, and all required extensions inside an isolated sandbox.

---

## 🛠️ Technology Stack

* **Backend API:** .NET 8.0 / 9.0 Web API, Docker (Linux Alpine Runtime)
* **Database Layer:** Azure Cosmos DB (NoSQL API)
* **Frontend SPA:** React 18+ (TypeScript), Vite
* **DevOps & Hosting:** GitHub Actions, Dev Containers, Azure Container Apps / Static Web Apps

---

## 📁 Repository Structure

```text
dev-rb-hub/symbio-hub/
├── .github/workflows/              # CI/CD validation and automation pipelines
├── backend/                        # .NET backend workspace
│   ├── Symbio.API/                 # Web API host and HTTP endpoints
│   ├── Symbio.API.Tests/           # Integration and endpoint tests
│   ├── Symbio.Core/                # Domain contracts, models, and services
│   ├── Symbio.Infrastructure/      # External integrations and repositories
│   └── Dockerfile                  # Backend dev container image
├── frontend/                       # React frontend workspace
│   └── Symbio.Frontend/            # Vite + TypeScript application
├── docs/                           # Contributor and product documentation
├── infrastructure/                 # IaC assets (Bicep and deployment config)
├── docker-compose.yml              # Local multi-service orchestration
├── AGENTS.md                       # Agent instructions entry point
├── CONTRIBUTING.md                 # Engineering and contribution guidelines
├── LICENSE                         # Apache License 2.0
└── README.md                       # Repository landing page
```

---

## 🚀 Getting Started & Orchestration Engines

Detailed setup and runtime instructions now live in docs:

- [Getting Started](docs/getting-started.md)
- [API Reference](docs/api-reference.md)

Configuration highlights:

- Local database and developer config defaults are documented in [Getting Started](docs/getting-started.md#database-setup-local).
- Pinch sandbox portal and credential setup are documented in [Getting Started](docs/getting-started.md#developer-configuration-defaults).

Quick start:

```powershell
docker compose up --build -d
```

- Frontend SPA: `http://localhost:5173`
- Backend Swagger: `http://localhost:5001/swagger`

---

## 🤝 Contributing

Review our [Contribution Guidelines](CONTRIBUTING.md) to understand branching conventions, pull request validations, and mock data parameters.

---

## 📄 License

This project is licensed under the **Apache License 2.0**. See the [LICENSE](LICENSE) file for comprehensive details.

---

## 🗺️ Product Roadmap & Strategic Horizons

Symbio Hub uses a role-isolated, state-aware delivery matrix. The development lifecycle is broken down below by core public/authenticated states, functional domains, and its native Australian financial engine powered by Pinch Payments.

### 🧩 Unified User Journey Epic Matrix

```text
                               ┌────────────────────────────────────────┐
                               │       Symbio Hub Gateway Router        │
                               └───────────────────┬────────────────────┘
                                                   │
                ┌──────────────────────────────────┴──────────────────────────────────┐
                ▼                                                                     ▼
    ┌───────────────────────┐                                             ┌───────────────────────┐
    │  LOGGED-OUT (PUBLIC)  │                                             │  LOGGED-IN (AUTHED)   │
    ├───────────────────────┤                                             ├───────────────────────┤
    │ • Read-Only Pitch UI  │                                             │ • Identity JWT Checks │
    │ • Masked Job Feeds    │                                             │ • Route Guard Matrix  │
    │ • Masked Talent Grid  │                                             │ • Role-Based Segments │
    └───────────┬───────────┘                                             └───────────┬───────────┘
                │                                                                     │
                └──────────────────────────────────┬──────────────────────────────────┘
                                                   │
         ┌─────────────────────────────────────────┼─────────────────────────────────────────┐
         ▼                                         ▼                                         ▼
┌─────────────────────────────────┐┌─────────────────────────────────┐┌─────────────────────────────────┐
│         ROLE: SME USER          ││      ROLE: FREELANCE EXPERT      ││      ROLE: PLATFORM ADMIN      │
├─────────────────────────────────┤├─────────────────────────────────┤├─────────────────────────────────┤
│ • ABN / Company Matching Registry││ • Professional Capability Profile││ • Multi-Tenant Escalation Desk  │
│ • Simplified Scope-of-Work (SoW)││ • Delivery Workbench UI         ││ • System Override Engine        │
│ • Spatial/Keyword Talent Search ││ • Milestone Completion Logging  ││ • Sub-Merchant Compliance Audit │
│ • Pinch BECS Direct Debit Setup ││ • Pinch Glassbox Account Sync   ││ • Automated Ledger Oversight    │
└─────────────────────────────────┘└─────────────────────────────────┘└─────────────────────────────────┘
```

### 📋 Comprehensive Functional Roadmap Tracking

> ✅ Epic 1 complete: public guest experience, structural marketing, and read-only job feed are implemented.
>
> ✅ Epic 2 complete: unified authentication handshake, route guard matrix, and session synchronization are implemented.
>
> ✅ Epic 3 complete: trust onboarding, user profiles, and verified SME/expert registration are implemented.
>
> ✅ Epic 4 complete: demand marketplace project posting, Cosmos-backed project storage, and SME scope wizard are implemented.
>
> ✅ Epic 5 complete: SME talent discovery, Cosmos-backed expert profile search, and verified regional talent matching are implemented.
>
> ✅ Epic 6 complete: expert delivery workbench, live SignalR log stream, and milestone update posting are implemented.
>
> ✅ Epic 7 complete: expert escrow onboarding, Pinch Glassbox account-link workflow, and onboarding verification state are implemented.
>
> ✅ Epic 8 complete: milestone settlement orchestration, pre-approval capture, and queued BECS debit execution are implemented.
>
> ✅ Epic 9 complete: accounting invoice feed synchronization, ledger status webhooks, and SME live accounting updates are implemented.
>
> ✅ Epic 10 complete: maintenance retainer contracts, flexible metered usage calculations, and recurring billing control center workflows are implemented.
>
> ✅ Epic 11 complete: platform operations command hub, admin compliance queue, and global safety override controls are implemented.
>
| Epic ID | Epic Category | Targeted User Roles | Core State Focus | Operational Engine | Target Project Module | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **01** | **Public Experience** | Guest / Anonymous | Logged-Out | Non-Payment | `Symbio.Frontend` / Public Pages | ✅ Done |
| **02** | **Session & Security** | All Roles | State Handshake | Non-Payment | `Symbio.Frontend` / Router Guards | ✅ Done |
| **03** | **Trust Onboarding** | SME & Expert | Logged-In | Non-Payment | `Symbio.API` / User Profiles | ✅ Done |
| **04** | **Demand Marketplace** | SME | Logged-In | Non-Payment | `Symbio.API` / Cosmos DB Jobs | ✅ Done |
| **05** | **Talent Discovery** | SME | Logged-In | Non-Payment | `Symbio.Infrastructure` / Cosmos NoSQL | ✅ Done |
| **06** | **Delivery Workbench** | Expert | Logged-In | Non-Payment | `Symbio.Frontend` / SignalR Logs | ✅ Done |
| **07** | **Escrow Onboarding** | Expert | Logged-In | **Pinch Payment** | `Symbio.Core` / Pinch Glassbox Hub | ✅ Done |
| **08** | **Milestone Settlement**| SME & Expert | Logged-In | **Pinch Payment** | `Symbio.Infrastructure` / Pinch BECS | ✅ Done |
| **09** | **Accounting Ledger** | SME | Logged-In | **Pinch Payment** | `Symbio.Infrastructure` / Accounting Sync | ✅ Done |
| **10** | **Retainer Management** | SME & Expert | Logged-In | **Pinch Payment** | `Symbio.API` / Pinch Subscriptions | ✅ Done |
| **11** | **Operations Command** | Platform Admin | Logged-In | Complete Ecosystem | `Symbio.API` / Admin Overrides | ✅ Done |

*💡 Note to Contributors: To link an active pull request to an epic milestone, append `Closes #<Epic-Issue-Number>` within your PR description body to trigger our automated repository status workflows.*


## 💖 Sponsor This Project

Symbio Hub is an open-source platform dedicated to bridging the digital divide for regional Australian SMEs. By sponsoring this project, you directly offset our baseline cloud infrastructure costs (Azure Container Apps, Cosmos DB, and telemetry logs) and help keep our regional talent ecosystem active.

### Choose Your Sponsorship Tier 🚀

| Tier | Monthly Impact | Perks |
| :--- | :--- | :--- |
| **🌱 Supporter**<br>`$5 AUD/mo` | Offsets baseline domain costs and DNS telemetry routing. | • Sponsor badge on your GitHub profile.<br>• Listed in our `CONTRIBUTORS.md` file. |
| **🚀 Ecosystem Builder**<br>`$15 AUD/mo` | Funds active staging databases for public community testing. | • Everything above.<br>• Your name linked in the repository README. |
| **🏗️ Production Partner**<br>`$45 AUD/mo` | **Fully covers active production hosting** & SignalR log streams. | • Everything above.<br>• **Your logo or name prominently featured below.**<br>• Priority review on your feature requests. |

<p align="center">
  <a href="https://github.com/sponsors/dev-rb-hub">
    <img src="https://img.shields.io/badge/Sponsor-dev--rb--hub-ea4aaa?logo=githubsponsors&logoColor=white" alt="Sponsor Symbio Hub on GitHub Sponsors" />
  </a>
</p>

Primary sponsorship profile: **https://github.com/sponsors/dev-rb-hub**

### 👑 Current Production Partners

*A huge thank you to the organisations keeping our regional innovation engine running:*

<!-- SPONSOR_LOGO_START -->
<p align="center">
  <a href="https://github.com/sponsors/dev-rb-hub"><strong>dev-rb-hub</strong></a>
  <br/>
  <i>Founding sponsor and project maintainer.</i>
</p>
<!-- SPONSOR_LOGO_END -->
