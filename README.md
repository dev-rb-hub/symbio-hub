# Symbio Hub

[![License](https://shields.io)](LICENSE)
[![GitHub Workflow Status](https://shields.io)](https://github.com)

Symbio Hub is a cross-platform, containerised open-source platform connecting regional SMEs with vetted technical talent. Powered by a decoupled .NET Core backend and a React frontend, the entire ecosystem is built to run seamlessly across Linux, macOS, and Windows via Docker virtualization.

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
│   └── Dockerfile       # Multi-stage production build script
├── frontend/            # Decoupled Single Page Application (SPA) Workspace
├── infrastructure/      # Infrastructure as Code (Azure Bicep / Container configurations)
├── CONTRIBUTING.md      # Local engineering guidelines & git practices
├── LICENSE              # Apache License 2.0
└── README.md            # Primary repository landing page
```

---

## 🚀 Getting Started

### 💻 Dev Container Workflow (Recommended)
1. Ensure **Docker Desktop** and VS Code's **Dev Containers extension** are installed.
2. Clone the repository and open it in VS Code.
3. Click **"Reopen in Container"** when prompted. Your development environment will configure automatically.

### 🛠️ Bare-Metal Manual Workflow
1. **Backend:** Navigate to `/backend`, configure your local secrets, and execute `dotnet run`.
2. **Frontend:** Navigate to `/frontend/Symbio.Frontend`, execute `npm install`, and run `npm run dev`.

---

## 🤝 Contributing

Review our [Contribution Guidelines](CONTRIBUTING.md) to understand branching conventions, pull request validations, and mock data parameters.

---

## 📄 License

This project is licensed under the **Apache License 2.0**. See the [LICENSE](LICENSE) file for comprehensive details.
