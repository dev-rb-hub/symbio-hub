# Symbio Hub Agent Instructions

This is the entry point for AI coding agents working in Symbio Hub. Start here, then read the narrower instructions for the area you are editing.

## Read first
- [README.md](README.md) for the product overview and architecture.
- [CONTRIBUTING.md](CONTRIBUTING.md) for contribution and validation expectations.
- [docker-compose.yml](docker-compose.yml) for local orchestration and service ports.
- [.github/workflows/ci.yml](.github/workflows/ci.yml) for the CI shape.
- [backend/.instructions.md](backend/.instructions.md) for backend changes.
- [frontend/Symbio.Frontend/.instructions.md](frontend/Symbio.Frontend/.instructions.md) for frontend changes.

## Boundaries
- Keep backend, frontend, and infrastructure work isolated.
- Do not leak backend data structures or internal API details into frontend code.
- Preserve the existing architecture and prefer small, localized changes.
- Link to existing docs instead of duplicating product or API details.

## Repo-specific workflow
- Prefer repository-native or containerized commands over ad hoc scripts.
- Local orchestration: `docker compose up --build -d`, `docker compose logs -f`, `docker compose down`.
- Backend validation: `dotnet test` and `dotnet run --project backend/Symbio.API`.
- Frontend validation: `cd frontend/Symbio.Frontend && npm run lint`, `npm run build`, and `npm run dev`.
- If a bug spans layers, trace it through the API, service, and repository flow before editing.

## Conventions
- Use branch prefixes `feature/`, `bugfix/`, `chore/`, and `docs/`.
- Follow Conventional Commits for pull requests.
- Keep infrastructure changes in `infrastructure/bicep/main.bicep` and nearby Bicep files.
- When adding a new capability, match the existing patterns in the surrounding layer rather than introducing a new abstraction.
