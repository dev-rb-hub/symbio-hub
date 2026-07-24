# Symbio Hub Agent Instructions

## Purpose
This is the root instruction file for AI coding agents working in Symbio Hub. Use it as the entry point, then read the narrower instructions for the area you are touching.

## Read First
- [README.md](README.md) for the product and repository overview.
- [CONTRIBUTING.md](CONTRIBUTING.md) for branch, commit, and validation conventions.
- [docker-compose.yml](docker-compose.yml) for local orchestration.
- [.github/workflows/ci.yml](.github/workflows/ci.yml) for repository validation shape.
- [backend/.instructions.md](backend/.instructions.md) for backend-specific work.
- [frontend/Symbio.Frontend/.instructions.md](frontend/Symbio.Frontend/.instructions.md) for frontend-specific work.

## Boundaries
- Keep `backend/`, `frontend/`, and `infrastructure/` isolated.
- Do not leak backend database structures or internal API details into frontend code.
- Preserve the existing architecture and keep changes minimal.
- Prefer linking to existing docs instead of repeating them here.

## Workflows
- Prefer repository-native or containerized commands when possible.
- Local orchestration: `docker compose up --build -d`, `docker compose logs -f`, `docker compose down`.
- Backend validation: `dotnet test`; local entry point: `dotnet run --project backend/Symbio.API`.
- Frontend validation: `cd frontend/Symbio.Frontend && npm run lint`, `npm run build`, `npm run dev`.

## Conventions
- Use branch prefixes `feature/`, `bugfix/`, `chore/`, and `docs/`.
- Follow Conventional Commits for PRs, for example `feat(payments): add pre-approval initialization`.
- Keep infrastructure changes in `infrastructure/bicep/main.bicep` and related IaC files.
