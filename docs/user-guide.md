# User Guide

This guide gives a practical walkthrough of the main Symbio Hub demo flows for each role.

## Role Overview

- Guest: Explore public positioning and read-only opportunities.
- SME: Post projects, prepare payment setup, and review delivery and settlement progress.
- Expert: Confirm onboarding and escrow readiness, then work from the delivery view.
- Admin: Monitor operational health, compliance, and diagnostics.

## Guest Flow

1. Open the frontend at `http://localhost:5173`.
2. Review the landing page messaging.
3. Explore public jobs and marketplace views.
4. Continue to login when ready to test role workflows.

## SME Flow

1. Sign in with an SME account from `/login`.
2. Complete trust onboarding if your session is newly seeded.
3. Create a project and publish the scope.
4. Review agreement, payment lifecycle, and settlement handoff screens as the project advances.
5. Use the billing and dashboard views to confirm Pinch runtime mode and payment state.
6. Open talent discovery to inspect available experts.
7. Use settings or profile to verify role and account details.

## Expert Flow

1. Sign in with an Expert account from `/login`.
2. Complete trust onboarding at `/onboarding` if required.
3. Open the expert dashboard to review assigned work.
4. Complete escrow onboarding and verify settlement readiness.
5. Use the delivery workbench to post live progress updates and milestone notes.
6. Use settings for role-specific account details.

## Admin Flow

1. Sign in using a seeded admin account.
2. Open the admin control center for an overview.
3. Review telemetry, compliance, agreements, and safety sections as needed.
4. Open settings to run the Pinch payment API diagnostics and review the terminal-style output.
5. Use `Refresh section` on the control center to reload only the active admin area.

## Additional Features

- Agreement approval: Shared SME, Expert, and Admin flow for reviewing and recording milestone approvals before delivery begins.
- Payment lifecycle demo: A focused view of the Pinch-backed milestone payment stages from approval through settlement.
- Settlement closeout: A closeout screen that shows whether completion evidence is sufficient to settle a milestone.
- Completion evidence matrix: Backend-supported evidence tracking for file hashes, git commits, and milestone readiness checks.
- Runtime mode panels: Key SME payment screens display whether Pinch is running in Mock, Sandbox, or Live mode.
- Admin Pinch diagnostics: The settings page includes a diagnostic summary plus terminal-style output for runtime-mode and sandbox verification checks.
- Webhook trust indicators: Webhook responses report trust outcome fields so operators can validate signature-handling behavior.

## Demo Notes

- Pinch runtime mode is visible on key SME payment surfaces: project pre-approval, recurring billing control center, and SME dashboard.
- Modes are shown as Mock, Sandbox, or Live, with credential status and response type context.
- In Mock mode, payment and settlement responses are simulated.
- Live financial behavior requires configured Pinch credentials.
- If runtime mode cannot be loaded, pages show a non-blocking warning so demo flow can continue.
- Webhook endpoints now return authenticity outcome fields (`trustState`, `trustReason`) so operators can confirm signature validation status in responses.

## Troubleshooting

- If pages fail to load data, use each page-level retry or refresh action.
- If access is denied unexpectedly, sign out and sign in again.
- If backend endpoints fail, verify API is running at `http://localhost:5001`.
- If Pinch checks are unclear, open admin settings and run the Pinch diagnostics again to inspect the terminal-style output and environment status.
- If the diagnostics report mock behavior, verify Pinch credentials and environment configuration before testing live payment scenarios.

## Related Documentation

- [Getting Started](getting-started.md)
- [Product Video Guides](product-video-guides.md)
- [API Reference](api-reference.md)
- [Competitor Landscape](competitor-landscape.md)
