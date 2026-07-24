# API Reference (Developer Quick Guide)

This quick guide lists key API and real-time endpoints used by the frontend and local testing workflows.

## Base URL

- Local API base URL: `http://localhost:5001`

## Authentication

- JWT auth is used for protected routes.
- Role-based access is enforced for SME, Expert, and Admin routes.
- Admin operations endpoints require the Admin role and master admin claim.

## Core endpoint groups

- `POST /api/Auth/register`
- `POST /api/Auth/login`
- `GET /api/Auth/verify-sme`

- `GET /api/Jobs`
- `POST /api/Project`
- `GET /api/Talent/search`

- `GET /api/ExpertWorkbench/overview`
- `POST /api/ExpertWorkbench/logs`

- `GET /api/payments/sme/invoices`
- `POST /api/payments/pre-approvals`
- `POST /api/payments/milestones/sign-off`

- `POST /api/retainers`
- `POST /api/retainers/{retainerId}/usage`
- `GET /api/retainers/control-center`

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

## SignalR hubs

- `GET /hubs/workbench`
- `GET /hubs/marketplace`
- `GET /hubs/accounting`

## Health and docs

- `GET /health`
- `GET /swagger`

For implementation details, inspect controllers in `backend/Symbio.API/Controllers` and endpoint mappings in `backend/Symbio.API/Endpoints`.
