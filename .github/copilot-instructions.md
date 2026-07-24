# Symbio Hub Copilot Instructions

This file helps AI coding agents understand the Symbio Hub repository and follow the project conventions.

## Use the root guidance file
- Refer to `AGENTS.md` for the repository overview, local workflows, and key conventions.
- Use the existing workspace documentation as ground truth: `README.md`, `CONTRIBUTING.md`, `docker-compose.yml`, and `.github/workflows/ci.yml`.

## What matters most
- Keep `/frontend`, `/backend`, and `/infrastructure` isolated.
- Do not expose backend database schemas or internal API details directly in frontend code.
- Prefer minimal, architecture-preserving changes.
- For frontend work, stay within `frontend/Symbio.Frontend` unless a cross-cutting change is required.
- For backend work, keep implementation inside `backend/` and avoid introducing frontend dependencies.

## Recommended commands
- `docker compose up --build -d`
- `docker compose logs -f`
- `docker compose down`
- `cd frontend/Symbio.Frontend && npm install && npm run dev`
- `cd frontend/Symbio.Frontend && npm run lint`
- `dotnet test` (backend)
- `dotnet run --project backend/Symbio.API` (backend, if backend sources exist)

## Links
- [AGENTS.md](../AGENTS.md)
- [README.md](../README.md)
- [CONTRIBUTING.md](../CONTRIBUTING.md)
- [docker-compose.yml](../docker-compose.yml)
- [.github/workflows/ci.yml](ci.yml)
