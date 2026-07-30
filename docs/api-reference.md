# API Reference (Developer Quick Guide)

This quick guide lists key API and real-time endpoints used by the frontend and local testing workflows.

## Base URL

- Local API base URL: `http://localhost:5001`

## Authentication

- JWT auth is used for protected routes.
- Role-based access is enforced for SME, Expert, and Admin routes.
- Admin operations endpoints require the Admin role and master admin claim.

## Core endpoint groups

- Auth
	- `POST /api/Auth/register`
	- `POST /api/Auth/login`
	- `GET /api/Auth/verify-sme`
- Public discovery and project setup
	- `GET /api/Jobs/public`
	- `GET /api/Jobs/{id}`
	- `GET /api/experts/search`
	- `GET /api/Talent`
	- `POST /api/Project`
	- `GET /api/Project`
	- `GET /api/Project/{id}`
- Onboarding and delivery
	- `GET /api/Onboarding/profile`
	- `POST /api/Onboarding/profile`
	- `GET /api/expert/dashboard`
	- `GET /api/ExpertWorkbench/overview`
	- `POST /api/ExpertWorkbench/logs`
	- `GET /api/payments/onboarding/status`
	- `POST /api/payments/onboarding/start`
	- `POST /api/payments/onboarding/refresh`
	- `POST /api/payments/onboarding/simulate-complete`
- Agreements, evidence, and settlement readiness
	- `GET /api/agreements`
	- `POST /api/agreements/upsert`
	- `POST /api/agreements/{id}/approve`
	- `POST /api/CompletionEvidence/file-hash`
	- `POST /api/CompletionEvidence/git-commit`
	- `GET /api/CompletionEvidence/milestone/{milestoneId}`
	- `GET /api/CompletionEvidence/epic/{epicId}`
	- `GET /api/CompletionEvidence/matrix`
	- `GET /api/CompletionEvidence/milestone/{milestoneId}/can-settle`
- Payments and billing
	- `GET /api/payments/runtime-mode`
	- `GET /api/payments/pinch/sandbox-verification`
	- `GET /api/payments/sme/invoices`
	- `POST /api/payments/pre-approvals`
	- `POST /api/payments/milestones/sign-off`
	- `POST /api/retainers`
	- `POST /api/retainers/{retainerId}/usage`
	- `GET /api/retainers/control-center`
- Webhooks
	- `POST /api/webhooks/pinch-settlements`
	- `POST /api/webhooks/accounting-invoices`
	- `POST /api/webhooks/pinch-subscriptions`

## Admin operations endpoints

- `GET /api/admin/telemetry/global`
- `GET /api/admin/compliance/queue`
- `POST /api/admin/compliance/flags`
- `POST /api/admin/compliance/reviews/{reviewId}/resolve`
- `GET /api/admin/overrides/safety-settings`
- `POST /api/admin/overrides/safety-settings`
- `POST /api/admin/overrides/users/{userId}/activation`

## Frontend-linked diagnostics behavior

- Admin users can run Pinch diagnostics from the settings page.
- That UI calls `GET /api/payments/runtime-mode` and `GET /api/payments/pinch/sandbox-verification` together.
- Use those responses to distinguish Mock, Sandbox, and Live behavior during demos and troubleshooting.

## SignalR hubs

- `/hubs/workbench`
- `/hubs/marketplace`
- `/hubs/accounting`

## Health and docs

- `GET /health`
- `GET /swagger`

For implementation details, inspect controllers in `backend/Symbio.API/Controllers` and endpoint mappings in `backend/Symbio.API/Endpoints`.
