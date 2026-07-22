# Contributing to Symbio Hub

Thank you for helping build financial ecosystems for regional NSW SMEs! 

## 🛣️ Architectural Pillars
* **Strict Decoupling:** Keep `/frontend`, `/backend`, and `/infrastructure` isolated. Never leak database structures into frontend repositories.
* **Pinch Payments Rails:** Use interface patterns when touching payment processors to ensure mocking remains seamless for public dev environments.

## 🚀 Branching & Commit Conventions
* **Branch Names:** Use prefixes: `feature/`, `bugfix/`, `chore/`, or `docs/`.
* **Commit Messages:** Follow Conventional Commits format (e.g., `feat(payments): add pre-approval initialization`).

## 🛠️ Local Quality Checks
* **Backend:** Run `dotnet test` and check compliance before opening a PR.
* **Frontend:** Execute `npm run lint` to catch syntax or formatting issues early.
