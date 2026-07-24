# Getting Started

This guide covers the supported local development workflows for Symbio Hub.

## Prerequisites

- Docker Desktop (for container workflows)
- .NET SDK 8.x (for bare-metal backend workflow)
- Node.js 20+ and npm (for bare-metal frontend workflow)

## Database setup (local)

The default local database is SQLite and is created automatically by the API at startup.

- Connection key: `ConnectionStrings:DefaultConnection`
- Default value: `Data Source=SymbioHub.db`
- Effective location (bare-metal): `backend/Symbio.API/SymbioHub.db`

No manual migration step is required for local development in the current setup because bootstrap SQL and seed routines run during startup.

## Developer configuration defaults

### Backend (`appsettings.Development.json`)

- `ConnectionStrings:DefaultConnection=Data Source=SymbioHub.db`
- `Jwt:Issuer=SymbioHub-Dev`
- `Jwt:Audience=SymbioHub-Clients`
- `Pinch:Environment=Sandbox`
- `Pinch:PortalUrl=https://sandbox.getpinch.com.au`
- `Pinch:ValidateWebhookSignature=false` (developer convenience)

Set `Pinch:ApiKey` and `Pinch:ApiSecret` with your sandbox credentials before testing live Pinch API paths.
For Pinch application authentication these values must be your **Application ID** and **Secret Key** (not Merchant ID).

Recommended developer mapping from Pinch Developer Portal:

- `Pinch:ApplicationId` <= **Application ID**
- `Pinch:SecretKey` <= **Secret Key**
- `Pinch:PortalUrl` => `https://sandbox.getpinch.com.au` for test mode

`Test Publishable Key` and `Redirect URIs` are portal-managed values and are not required by the current server-to-server client-credentials flow.

Optional:

- `Pinch:TokenScope` can be set if your Pinch application requires a scope. By default, Symbio only sends `grant_type=client_credentials` to match the Pinch auth guide.

### Frontend default backend connection

The frontend now defaults to a local proxy path in development:

- `VITE_API_BASE_URL` optional override
- Default when unset in dev: `/api-proxy`

Vite proxies this to the backend target (`http://localhost:5001` by default).

For Docker Compose, the proxy target is set to `http://symbio-backend:8080` so frontend and backend connect without extra manual configuration.

## Option A: Docker Compose (recommended)

From the repository root:

```powershell
docker compose up --build -d
docker compose logs -f
docker compose down
```

Notes:

- The backend container runs on `http://localhost:5001`.
- The frontend container runs on `http://localhost:5173`.
- Source folders are bind-mounted for iterative development.

If Docker is not running, Compose commands will fail to connect to the Docker engine. Start Docker Desktop first.

## Option B: VS Code Dev Container

1. Install Docker Desktop.
2. Install the VS Code Dev Containers extension.
3. Open the repository in VS Code.
4. Choose **Reopen in Container** when prompted.

## Option C: Bare-metal local run

### Backend API

From the repository root:

```powershell
dotnet run --project backend/Symbio.API/Symbio.API.csproj
```

### Frontend SPA

From the repository root:

```powershell
cd frontend/Symbio.Frontend
npm install
npm run dev
```

## Runtime URLs

- Frontend SPA: `http://localhost:5173`
- Backend Swagger: `http://localhost:5001/swagger`
- Health endpoint: `http://localhost:5001/health`
- Pinch sandbox portal: `https://sandbox.getpinch.com.au`
- SignalR hubs:
  - Workbench: `http://localhost:5001/hubs/workbench`
  - Marketplace: `http://localhost:5001/hubs/marketplace`
  - Accounting: `http://localhost:5001/hubs/accounting`

## Validation commands

From repository root:

```powershell
dotnet test backend/Symbio.API.Tests/Symbio.API.Tests.csproj
cd frontend/Symbio.Frontend
npm run lint
npm run build
```
