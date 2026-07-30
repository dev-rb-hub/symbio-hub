# User Guide

This guide gives a practical walkthrough of the Symbio Hub demo flows for each role.

## Role Overview

- Guest: Explore public positioning and read-only opportunities.
- SME: Post projects, set payment pre-approvals, and track invoice/payment states.
- Expert: Confirm onboarding and escrow readiness, then work from the delivery view.
- Admin: Monitor compliance, telemetry, and safety settings.

## Guest Flow

1. Open the frontend at `http://localhost:5173`.
2. Review the landing page messaging.
3. Open public jobs and marketplace views.
4. Continue to login when ready to test role workflows.

## SME Flow

1. Sign in with an SME account from `/login`.
2. Open `Project New` and complete the scope form.
3. Publish the project and complete payment pre-approval when prompted.
4. Open the SME dashboard to review invoice and payment state updates.
5. Visit talent discovery to inspect available experts.

## Expert Flow

1. Sign in with an Expert account from `/login`.
2. Open escrow onboarding at `/escrow/onboarding`.
3. Start onboarding and observe status updates.
4. Open the workbench to review delivery assignments and logs.

## Admin Flow

1. Sign in using a seeded admin account.
2. Open `/admin/control` for an overview.
3. Navigate to telemetry, compliance queue, and safety overrides.
4. Use `Refresh section` to reload only the current admin section.

## Demo Notes

- Payment mode is shown in pre-approval flows as Mock, Sandbox, or Live.
- In Mock mode, payment and settlement responses are simulated.
- Live financial behavior requires configured Pinch credentials.

## Troubleshooting

- If pages fail to load data, use each page-level retry action.
- If access is denied unexpectedly, sign out and sign in again.
- If backend endpoints fail, verify API is running at `http://localhost:5001`.

## Related Documentation

- [Getting Started](getting-started.md)
- [API Reference](api-reference.md)
- [Competitor Landscape](competitor-landscape.md)
