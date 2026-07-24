# Symbio Hub

[![License](https://shields.io)](LICENSE)
[![GitHub Workflow Status](https://shields.io)](https://github.com)

Symbio Hub is a cross-platform, containerised open-source platform connecting regional SMEs with vetted technical talent. Powered by a decoupled .NET Core backend and a React frontend, the entire ecosystem runs seamlessly across Linux, macOS, and Windows via Docker virtualization.

---

## 🎯 Problem & Market Solution

* **The Challenge:** Regional SMEs lack access to trusted local digital expertise, leaving them dependent on expensive metropolitan agencies or high-risk offshore outsourcing.
* **The Solution:** A secure, localized marketplace featuring:
    * **Verified Trust Profiles:** KYC-enabled expert registration profiles.
    * **Risk-Free Escrow:** Milestone-based payments powered by [Pinch Payments](https://getpinch.com.au).
    * **AI Modernization:** Tailored deployment tracks for local AI and automation workflows.

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
├── .devcontainer/       # Unified VS Code Developer Container configuration
├── .github/workflows/   # CI/CD validation and automated cloud deployment pipelines
├── backend/             # Containerised .NET Core Microservices / APIs
│   ├── Symbio.API/      # Primary Web API delivery layer
│   └── Dockerfile       # Multi-stage production build script
├── frontend/            # Decoupled Single Page Application (SPA) Workspace
│   └── Symbio.Frontend/ # React Web UI (Vite / TypeScript)
├── infrastructure/      # Infrastructure as Code (Azure Bicep / Container configurations)
├── .editorconfig        # Global team linting and styling layout engine
├── docker-compose.yml   # Multi-service runtime local orchestration matrix
├── CONTRIBUTING.md      # Local engineering guidelines & git practices
├── LICENSE              # Apache License 2.0
└── README.md            # Primary repository landing page
```

---

## 🚀 Getting Started & Orchestration Engines

### 🐳 Option A: Docker Compose Workflow (Recommended)
This workflow launches both the .NET backend API and the React Vite frontend concurrently in independent Linux containers. **Hot-reloading is configured out-of-the-box**: changes made on your host machine will immediately reflect inside the active runtime containers.

Ensure **Docker Desktop** is running, open a Windows PowerShell window in the project root, and execute:

```powershell
# 1. Build infrastructure images and run the environment in detached mode
docker compose up --build -d

# 2. View and stream active container compilation or runtime console logs
docker compose logs -f

# 3. Halt the ecosystem containers and safely teardown communication sockets
docker compose down
```
* Access the Frontend SPA at: `http://localhost:5173`
* Access the Backend Swagger API documentation at: `http://localhost:5001/swagger`

### 💻 Option B: Dev Container Workflow
1. Ensure **Docker Desktop** and VS Code's **Dev Containers extension** are active.
2. Clone the repository and open the workspace in VS Code.
3. Select **"Reopen in Container"** when prompted. Runtimes install automatically inside your sandbox.

### 🛠️ Option C: Bare-Metal Manual Workflow
1. **Backend:** Navigate to `/backend`, configure local appsecrets, and execute `dotnet run --project Symbio.API`.
2. **Frontend:** Navigate to `/frontend/Symbio.Frontend`, execute `npm install`, and run `npm run dev`.

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

| Epic ID | Epic Category | Targeted User Roles | Core State Focus | Operational Engine | Target Project Module | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **01** | **Public Experience** | Guest / Anonymous | Logged-Out | Non-Payment | `Symbio.Frontend` / Public Pages | #1 |
| **02** | **Session & Security** | All Roles | State Handshake | Non-Payment | `Symbio.Frontend` / Router Guards | #2 |
| **03** | **Trust Onboarding** | SME & Expert | Logged-In | Non-Payment | `Symbio.Infrastructure` / Auth0 & ABR | #3 |
| **04** | **Demand Marketplace** | SME | Logged-In | Non-Payment | `Symbio.API` / Cosmos DB Jobs | #4 |
| **05** | **Talent Discovery** | SME | Logged-In | Non-Payment | `Symbio.Infrastructure` / Cosmos NoSQL | #5 |
| **06** | **Delivery Workbench** | Expert | Logged-In | Non-Payment | `Symbio.Frontend` / SignalR Logs | #6 |
| **07** | **Escrow Onboarding** | Expert | Logged-In | **Pinch Payment** | `Symbio.Core` / Pinch Glassbox Hub | #7 |
| **08** | **Milestone Settlement**| SME & Expert | Logged-In | **Pinch Payment** | `Symbio.Infrastructure` / Pinch BECS | #8 |
| **09** | **Accounting Ledger** | SME | Logged-In | **Pinch Payment** | `Symbio.Infrastructure` / Accounting Sync | #9 |
| **10** | **Retainer Management** | SME & Expert | Logged-In | **Pinch Payment** | `Symbio.API` / Pinch Subscriptions | #10 |
| **11** | **Operations Command** | Platform Admin | Logged-In | Complete Ecosystem | `Symbio.API` / Admin Overrides | #11 |

*💡 Note to Contributors: To link an active pull request to an epic milestone, append `Closes #<Epic-Issue-Number>` within your PR description body to trigger our automated repository status workflows.*

