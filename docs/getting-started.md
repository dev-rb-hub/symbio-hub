# Getting Started

This guide covers the supported local development workflows for Symbio Hub.

## Prerequisites

- Docker Desktop (for container workflows)
- .NET SDK 8.x (for bare-metal backend workflow)
- Node.js 20+ and npm (for bare-metal frontend workflow)

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
