# Symbio Hub Agent Instructions

## Purpose
This file helps AI coding agents understand the Symbio Hub repository, the project boundaries, and the preferred local workflows. It is intentionally concise and links to existing documentation for details.

## Repository overview
Symbio Hub is a container-first, cross-platform application with three main areas:
- `backend/` — .NET Web API services and infrastructure libraries.
- `frontend/` — React + TypeScript SPA using Vite.
- `infrastructure/` — Azure Bicep provisioning and deployment-related IaC.
- `docker-compose.yml` — local orchestration for backend and frontend development.

## Key conventions
- Keep `/frontend`, `/backend`, and `/infrastructure` isolated.
- Do not leak backend database structures or internal APIs directly into frontend code.
- Use branch prefixes: `feature/`, `bugfix/`, `chore/`, `docs/`.
- Follow Conventional Commits for PRs (e.g. `feat(payments): add pre-approval initialization`).

## Local workstreams
Prefer containerized or repository-native commands when available.

### Docker Compose development
- `docker compose up --build -d`
- `docker compose logs -f`
- `docker compose down`

### Frontend
- `cd frontend/Symbio.Frontend`
- `npm install`
- `npm run dev`
- `npm run lint`
- `npm run build`

### Backend
- Run the backend tests and validate API changes before PRs.
- Use `dotnet test` in the backend workspace if applicable.
- `dotnet run --project backend/Symbio.API` is the expected development entry path when backend sources are present.

## Agent guidance
- Prefer minimal code changes and preserve existing architecture.
- For frontend work, stay within `frontend/Symbio.Frontend` unless a cross-cutting change is required.
- For backend work, keep implementation inside `backend/` and avoid introducing frontend dependencies.
- Use the existing docs as ground truth rather than reproducing them.

## Links
- [README.md](README.md)
- [CONTRIBUTING.md](CONTRIBUTING.md)
- [docker-compose.yml](docker-compose.yml)
- [.github/workflows/ci.yml](.github/workflows/ci.yml)
- [infrastructure/bicep/main.bicep](infrastructure/bicep/main.bicep)
