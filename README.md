# Symbio Hub

[![License: Apache 2.0](https://shields.io)](LICENSE)
[![GitHub Workflow Status](https://shields.io)](https://github.com)

Symbio Hub is an open-source digital bridge designed to connect independent technology professionals with Small and Medium Enterprises (SMEs) across regional New South Wales. Powered by a decoupled .NET backend and a React frontend, Symbio Hub enables regional businesses to source vetted local talent, adopt AI automation, and execute milestones securely using Australian-native payment rails.

---

## 🎯 Problem & Market Solution

### The Challenge in Regional NSW
SMEs across regional New South Wales face an operational crisis: keeping pace with rapid digital transformation (such as AI workflow automation) is limited by a lack of access to vetted, reliable local technical talent. Businesses are frequently forced to choose between expensive metropolitan agencies or high-risk offshore outsourcing.

### The Symbio Hub Solution
Symbio Hub eliminates regional outsourcing risk by providing a secure, localized marketplace framework:
* **Verified Trust Profiles:** Integrated identity verification (KYC) mechanisms ensure SMEs deal exclusively with legitimate local operators.
* **Risk-Free Milestone Escrow:** Powered by Pinch Payments, project funds are safely committed upfront and only released to providers upon successful, verified milestone execution.
* **Tangible AI Modernization:** Built to streamline short-cycle projects—such as integrating secure AI customer assistants or predictive inventory trackers—saving businesses critical manual administration hours.

---

## 🛠️ Technology Stack

### Frontend
* **Core:** React 18+ with TypeScript
* **Build Tool:** Vite
* **Hosting:** Azure Static Web Apps (Free Tier)
* **CI/CD:** GitHub Actions (Automated build and sync)

### Backend
* **Core:** .NET 8.0 / 9.0 Web API (C#)
* **Database:** Azure Cosmos DB (NoSQL API)
* **Payment Engine:** Pinch Payments SDK (PayFac Architecture)
* **Hosting:** Azure App Service / Azure Functions
* **Secrets Management:** Azure Key Vault

---

## 📁 Repository Structure

The project enforces strict separation of concerns using a decoupled monorepo structure:

```text
dev-rb-hub/symbio-hub/
├── .github/
│   └── workflows/
│       ├── frontend-deploy.yml    # Auto-deploys React to Azure Static Web Apps
│       └── backend-deploy.yml     # Auto-deploys .NET to Azure App Service
├── backend/                       # Isolated .NET Core Microservices / APIs
│   ├── Symbio.API/                # REST Controllers, Program.cs, Swagger (OpenAPI)
│   ├── Symbio.Core/               # Domain Models, Interfaces, Pinch Payment logic
│   └── Symbio.Infrastructure/     # Cosmos DB Data Access, Third-Party Clients
├── frontend/                      # Decoupled SPA UI Workspace
│   └── Symbio.Frontend/           # React Web Application (Vite / TypeScript)
├── infrastructure/                # Platform Infrastructure as Code (IaC)
│   └── bicep/                     # Azure Resource Manager templates
├── LICENSE                        # Apache License 2.0
└── README.md                      # Developer onboarding & local setup guide
```

---

## 💳 Pinch Payment Architecture Matrix

Symbio Hub embeds **Pinch Payments** to run natively on local Australian financial rails (Credit Cards and BECS Direct Debit). This architecture replaces high-cost global processors with targeted B2B automation tools.

| Feature Focus | Specific Pinch API / Product | Value to Symbio Hub |
|---|---|---|
| **Escrow & Splits** | Glassbox Merchant Hub & Webhooks | Isolates contract funds securely and auto-deducts the 10% platform fee. |
| **B2B Trust** | BECS Direct Debit Payments | Allows regional SMEs to fund large $5,000+ tech contracts securely via local bank routing. |
| **Project Milestones** | Customer Pre-Approvals Tool | Authorizes automated billing the moment code passes deployment testing. |

### Managed Merchant Onboarding
When a local technical expert registers, they are provisioned as an independent "Managed Merchant" (sub-account) via the backend API. 
1. **Validate:** The platform checks the expert's Australian Business Number (ABN).
2. **Provision:** The backend calls Pinch via the .NET SDK to create an isolated sub-account with 24-hour activation.
3. **Verify:** The API returns a secure, hosted Pinch setup URL where the expert completes identity verification (KYC) and links their bank account.

---

## 🚀 Getting Started (Local Development)

### Prerequisites
* [Node.js (v18 or higher)](https://nodejs.org)
* [.NET 8.0 / 9.0 SDK](https://microsoft.com)
* IDE of choice (VS Code, Visual Studio 2022, or JetBrains Rider)
* Optional: [Azure Cosmos DB Emulator](https://microsoft.com)

### Backend Setup
1. **Navigate** to the backend directory:
   ```bash
   cd backend
   ```
2. **Restore** NuGet dependencies:
   ```bash
   dotnet restore
   ```
3. **Configure** local settings. Create an `appsettings.Development.json` file inside `Symbio.API/`:
   ```json
   {
     "CosmosDb": {
       "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
       "DatabaseName": "SymbioHubDev"
     },
     "PinchPayments": {
       "ApiKey": "YOUR_SANDBOX_KEY",
       "IsSandbox": true
     }
   }
   ```
   *Note: If no local emulator is active, the app falls back to a mocked service layer for open-source contributors.*
4. **Run** the API:
   ```bash
   dotnet run --project Symbio.API
   ```

### Frontend Setup
1. **Navigate** to the frontend directory:
   ```bash
   cd frontend/Symbio.Frontend
   ```
2. **Install** node modules:
   ```bash
   npm install
   ```
3. **Configure** environments. Create a `.env.local` file:
   ```env
   VITE_API_BASE_URL=http://localhost:5001
   ```
4. **Launch** Vite development server:
   ```bash
   npm run dev
   ```

---

## 🤖 Automated Security & Governance
This repository uses automated governance guards to maintain code safety before merging:
* **Dependabot Scanning:** Configured weekly to parse `npm` and `nuget` ecosystems for outdated or vulnerable packages.
* **Secret Interception:** Automated branch policies reject commits containing exposed API tokens or plain-text connection strings.

---

## 📄 License

Distributed under the Apache License 2.0. See `LICENSE` for more details.
