# Symbio Hub Copilot Instructions

This file is the editor-facing entry point for AI coding agents. Read [AGENTS.md](../AGENTS.md) first, then the narrower instructions for the area you are touching.

## Use the right scope
- For backend work, follow [backend/.instructions.md](../backend/.instructions.md).
- For frontend work, follow [frontend/Symbio.Frontend/.instructions.md](../frontend/Symbio.Frontend/.instructions.md).
- For broader repository conventions, use the root docs: [README.md](../README.md), [CONTRIBUTING.md](../CONTRIBUTING.md), [docker-compose.yml](../docker-compose.yml), and [ci.yml](ci.yml).

## Keep in mind
- Keep `backend/`, `frontend/`, and `infrastructure/` isolated.
- Do not expose backend database schemas or internal API details in frontend code.
- Prefer minimal, architecture-preserving changes and link to existing docs instead of duplicating them here.
