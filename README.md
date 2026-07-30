# Symbio Hub

<p align="center">
  <a href="https://github.com/dev-rb-hub/symbio-hub">
    <img src="https://repository-images.githubusercontent.com/1308285720/cab93091-5614-4e02-8286-2cd93adf0cdc" alt="Symbio Hub Social Preview" width="650" />
  </a>
</p>

[![License](https://shields.io)](LICENSE)
[![GitHub Workflow Status](https://shields.io)](https://github.com/dev-rb-hub/symbio-hub/actions/workflows/ci.yml)

Regional businesses deserve trusted local digital expertise—not anonymous global marketplaces. 
Symbio Hub provides a secure marketplace where verified local professionals collaborate with confidence through protected payments and AI-enabled modernization

Symbio Hub is a cross-platform, containerised open-source platform connecting regional SMEs with vetted technical talent. Powered by a decoupled .NET Core backend and a React frontend, the entire ecosystem runs seamlessly across Linux, macOS, and Windows via Docker virtualization.

---

## 🔗 Quick Links

- [Getting Started](docs/getting-started.md)
- [User Guide](docs/user-guide.md)
- [Hackathon Pitch](docs/pinch-pitch.md)
- [Competitor Landscape](docs/competitor-landscape.md)
- [API Reference](docs/api-reference.md)
- [Contributing](CONTRIBUTING.md)

---

## 🎯 Problem & Market Solution

* **The Challenge:** Regional SMEs lack access to trusted local digital expertise, leaving them dependent on expensive metropolitan agencies or high-risk offshore outsourcing.
* **The Solution:** A secure, localized marketplace featuring:
    * **Verified Trust Profiles:** KYC-enabled expert registration profiles.
    * **Risk-Free Escrow:** Milestone-based payments powered by [Pinch Payments](https://getpinch.com.au).
    * **AI Modernization:** Tailored deployment tracks for local AI and automation workflows.

Product Pitch - see [Hackathon Pitch](docs/pinch-pitch.md).
For competitor context and market positioning, see [Competitor Landscape](docs/competitor-landscape.md).

<p align="center">
  <a href="https://github.com/dev-rb-hub/symbio-hub-flyer">
    <img src="https://github.com/dev-rb-hub/symbio-hub/blob/main/frontend/Symbio.Frontend/src/assets/images/Symbio-hub%20flyer.png" alt="Symbio Hub Flyer" width="650" />
  </a>
</p>

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
├── .devcontainer/                  # Optional local containerised dev environment
├── .github/                        # Workflow automation and Copilot repo guidance
├── backend/                        # .NET backend workspace
│   ├── Symbio.API/                 # Web API host and HTTP endpoints
│   ├── Symbio.API.Tests/           # Integration and endpoint tests
│   ├── Symbio.Core/                # Domain contracts, models, and services
│   ├── Symbio.Infrastructure/      # External integrations and repositories
│   └── Dockerfile                  # Backend dev container image
├── frontend/                       # React frontend workspace
│   └── Symbio.Frontend/            # Vite + TypeScript application
├── docs/                           # Product guides, setup notes, and API docs
├── infrastructure/                 # IaC assets including Bicep deployment files
├── docker-compose.yml              # Local multi-service orchestration
├── AGENTS.md                       # Agent instructions entry point
├── CONTRIBUTING.md                 # Engineering and contribution guidelines
├── LICENSE                         # Apache License 2.0
└── README.md                       # Repository landing page
```

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

For detailed setup, environment configuration, and role-by-role demo walkthroughs, use:

- [Getting Started](docs/getting-started.md)
- [User Guide](docs/user-guide.md)
- [API Reference](docs/api-reference.md)

---

## 🤝 Contributing

Review our [Contribution Guidelines](CONTRIBUTING.md) to understand branching conventions, pull request validations, and mock data parameters.

---

## 📄 License

This project is licensed under the **Apache License 2.0**. See the [LICENSE](LICENSE) file for comprehensive details.

---

## 🗺️ Product Roadmap Snapshot

Current roadmap outcomes are implemented across the completed Epic 01 to Epic 11 delivery stream, including:

- Public guest and auth role routing flows.
- SME project posting, talent discovery, and agreement handoff flows.
- Expert delivery workbench, escrow onboarding, and live progress updates.
- Pinch payment runtime visibility, milestone lifecycle, settlement closeout, and recurring billing.
- Completion evidence capture and settlement readiness checks.
- Admin compliance, operational controls, and Pinch diagnostics from role settings.

For roadmap-level details and live issue status, use:

- [Symbio Hub Roadmap Project](https://github.com/users/dev-rb-hub/projects/1)
- [Repository Issues](https://github.com/dev-rb-hub/symbio-hub/issues)

*Contributor note: include `Closes #<Issue-Number>` in PR descriptions to auto-close roadmap stories.*


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
