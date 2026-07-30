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
4. If either public page fails initial load, use `Retry`; if a page is empty, use `Refresh`.
5. Continue to login when ready to test role workflows.

## SME Flow

1. Sign in with an SME account from `/login`.
2. Complete trust onboarding at `/onboarding` if your session is newly seeded.
3. Open `/project/new` and complete the scope form.
4. Publish the project and complete payment pre-approval when prompted.
5. Check the Pinch runtime mode panel (Mock, Sandbox, or Live) on the project flow before confirming payment actions.
6. Open `/billing/control-center` to review recurring billing controls and confirm the same runtime mode context.
7. Open `/sme/dashboard` to review invoice/payment state updates and verify runtime mode visibility there.
8. Visit `/talent/discovery` to inspect available experts.
9. Use `/profile` to verify role and account details.

## Expert Flow

1. Sign in with an Expert account from `/login`.
2. Complete trust onboarding at `/onboarding` if required.
3. Open `/expert/dashboard` to search and filter Projects, Milestones, Payments, and Reports.
4. Open escrow onboarding at `/escrow/onboarding` and verify settlement readiness.
5. Open `/expert/workbench` to post live delivery updates and progress logs.
6. Use `/settings` for role-specific account settings.

## Admin Flow

1. Sign in using a seeded admin account.
2. Open `/admin/control` for an overview.
3. Navigate to `/admin/telemetry` for runtime and activity visibility.
4. Navigate to `/admin/compliance` for review queue operations.
5. Navigate to `/admin/safety` for safety override controls.
6. Use `/settings` to verify your admin account details.
7. Use `Refresh section` to reload only the current admin section.

## Demo Notes

- Pinch runtime mode is visible on key SME payment surfaces: project pre-approval, recurring billing control center, and SME dashboard.
- Modes are shown as Mock, Sandbox, or Live, with credential status and response type context.
- In Mock mode, payment and settlement responses are simulated.
- Live financial behavior requires configured Pinch credentials.
- If runtime mode cannot be loaded, pages show a non-blocking warning so demo flow can continue.
- Webhook endpoints now return authenticity outcome fields (`trustState`, `trustReason`) so operators can confirm signature validation status in responses.

## Troubleshooting

- If pages fail to load data, use each page-level retry action.
- If access is denied unexpectedly, sign out and sign in again.
- If backend endpoints fail, verify API is running at `http://localhost:5001`.

## Related Documentation

- [Getting Started](getting-started.md)
- [API Reference](api-reference.md)
- [Competitor Landscape](competitor-landscape.md)
