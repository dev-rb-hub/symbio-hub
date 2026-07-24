CREATE TABLE IF NOT EXISTS Jobs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    ClientName TEXT NOT NULL,
    ClientSurname TEXT NOT NULL,
    Budget REAL NOT NULL,
    ContactEmail TEXT NOT NULL,
    IsPublished INTEGER NOT NULL DEFAULT 1,
    PostedAt TEXT NOT NULL
);

-- Cosmos-backed collections are provisioned separately and are not represented here.
-- See infrastructure/bicep/main.bicep for Projects and TalentProfiles containers.

CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CompanyName TEXT NOT NULL DEFAULT '',
    BusinessIdentifier TEXT NOT NULL DEFAULT '',
    ProfileSummary TEXT NOT NULL DEFAULT '',
    OnboardingCompleted INTEGER NOT NULL DEFAULT 0,
    OnboardedAt TEXT
);

CREATE TABLE IF NOT EXISTS DeliveryAssignments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ExpertEmail TEXT NOT NULL,
    ProjectTitle TEXT NOT NULL,
    ClientName TEXT NOT NULL,
    Category TEXT NOT NULL,
    ScopeSummary TEXT NOT NULL,
    CurrentMilestone TEXT NOT NULL,
    Status TEXT NOT NULL,
    ProgressPercent INTEGER NOT NULL,
    Priority TEXT NOT NULL,
    DueDate TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS DeliveryLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DeliveryAssignmentId INTEGER NOT NULL,
    ExpertEmail TEXT NOT NULL,
    CreatedByEmail TEXT NOT NULL,
    Level TEXT NOT NULL,
    Message TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS EscrowOnboardingProfiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ExpertEmail TEXT NOT NULL UNIQUE,
    ProviderAccountId TEXT NOT NULL,
    Status TEXT NOT NULL,
    OnboardingUrl TEXT NOT NULL,
    LastSyncedAtUtc TEXT NOT NULL,
    OnboardedAtUtc TEXT
);

CREATE TABLE IF NOT EXISTS ProjectPaymentStateRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId TEXT NOT NULL UNIQUE,
    State TEXT NOT NULL,
    GrossAmount REAL NOT NULL,
    PlatformFeeAmount REAL NOT NULL,
    ContractorAmount REAL NOT NULL,
    Currency TEXT NOT NULL,
    LastProviderReference TEXT,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS PaymentPreApprovals (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId TEXT NOT NULL,
    MilestoneId TEXT NOT NULL,
    ClientEmail TEXT NOT NULL,
    Amount REAL NOT NULL,
    Currency TEXT NOT NULL,
    BsbMasked TEXT NOT NULL,
    AccountNumberMasked TEXT NOT NULL,
    Status TEXT NOT NULL,
    ProviderPreApprovalId TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    ApprovedAtUtc TEXT
);

CREATE TABLE IF NOT EXISTS DirectDebitPullRequests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId TEXT NOT NULL,
    MilestoneId TEXT NOT NULL,
    PreApprovalProviderId TEXT NOT NULL,
    Amount REAL NOT NULL,
    Currency TEXT NOT NULL,
    Status TEXT NOT NULL,
    ProviderDebitId TEXT,
    LastError TEXT,
    RequestedAtUtc TEXT NOT NULL,
    ProcessedAtUtc TEXT
);

CREATE TABLE IF NOT EXISTS AccountingInvoices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId TEXT NOT NULL,
    MilestoneId TEXT NOT NULL,
    ClientEmail TEXT NOT NULL,
    Provider TEXT NOT NULL,
    ProviderInvoiceId TEXT NOT NULL,
    InvoiceNumber TEXT NOT NULL,
    Status TEXT NOT NULL,
    TotalAmount REAL NOT NULL,
    Currency TEXT NOT NULL,
    LedgerPayloadJson TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS RetainerContracts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId TEXT NOT NULL,
    MilestoneId TEXT NOT NULL,
    ClientEmail TEXT NOT NULL,
    ExpertEmail TEXT NOT NULL,
    ProviderPlanId TEXT NOT NULL,
    ProviderSubscriptionId TEXT NOT NULL,
    BaseMonthlyAmount REAL NOT NULL,
    Currency TEXT NOT NULL,
    IncludedSupportHours REAL NOT NULL,
    IncludedCloudUnits REAL NOT NULL,
    OverageRatePerHour REAL NOT NULL,
    OverageRatePerCloudUnit REAL NOT NULL,
    Status TEXT NOT NULL,
    NextBillingAtUtc TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS RetainerUsages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RetainerContractId INTEGER NOT NULL,
    SupportHours REAL NOT NULL,
    CloudUnits REAL NOT NULL,
    PeriodStartUtc TEXT NOT NULL,
    PeriodEndUtc TEXT NOT NULL,
    ProcessedForBilling INTEGER NOT NULL DEFAULT 0,
    CreatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS RetainerCharges (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RetainerContractId INTEGER NOT NULL,
    ProviderSubscriptionId TEXT NOT NULL,
    BaseAmount REAL NOT NULL,
    MeteredAmount REAL NOT NULL,
    TotalAmount REAL NOT NULL,
    Currency TEXT NOT NULL,
    Status TEXT NOT NULL,
    ProviderReference TEXT,
    ChargedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS AdminProjectFlagRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId TEXT NOT NULL,
    MilestoneId TEXT NOT NULL,
    ReportedByEmail TEXT NOT NULL,
    Severity TEXT NOT NULL,
    Reason TEXT NOT NULL,
    Status TEXT NOT NULL,
    ResolvedByEmail TEXT,
    CreatedAtUtc TEXT NOT NULL,
    ResolvedAtUtc TEXT
);

CREATE TABLE IF NOT EXISTS AdminUserComplianceRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserEmail TEXT NOT NULL,
    UserRole TEXT NOT NULL,
    ReviewStatus TEXT NOT NULL,
    RiskLevel TEXT NOT NULL,
    Notes TEXT NOT NULL,
    ReviewedByEmail TEXT,
    CreatedAtUtc TEXT NOT NULL,
    ReviewedAtUtc TEXT
);

CREATE TABLE IF NOT EXISTS AdminSafetySettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingKey TEXT NOT NULL UNIQUE,
    SettingValue TEXT NOT NULL,
    UpdatedByEmail TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS AdminAuditLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AdminEmail TEXT NOT NULL,
    Action TEXT NOT NULL,
    TargetType TEXT NOT NULL,
    TargetReference TEXT NOT NULL,
    DetailJson TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL
);

INSERT INTO Jobs (Title, Description, ClientName, ClientSurname, Budget, ContactEmail, IsPublished, PostedAt)
VALUES
('Regional Retail Website Refresh', 'Build a mobile-first homepage and checkout experience for a small NSW retail brand.', 'Harper', 'Bright', 9500.00, 'contact@harperbright.com', 1, '2026-07-19T00:00:00Z'),
('Local Healthcare Data Dashboard', 'Create a lightweight reporting dashboard for a regional practice using anonymised patient metrics.', 'Jade', 'Taylor', 14500.00, 'jade.taylor@coastalhealth.au', 1, '2026-07-12T00:00:00Z'),
('Food Delivery Loyalty Campaign', 'Design and build a customer loyalty landing page with signup flow and campaign analytics.', 'Miles', 'Kerr', 7200.00, 'miles@harvestdeli.au', 1, '2026-07-22T00:00:00Z');

INSERT INTO DeliveryAssignments (ExpertEmail, ProjectTitle, ClientName, Category, ScopeSummary, CurrentMilestone, Status, ProgressPercent, Priority, DueDate, IsActive, UpdatedAt)
VALUES
('expert@example.com', 'Regional Retail Website Refresh', 'Harper Bright', 'Web Experience', 'Rebuild the public homepage and checkout flow with mobile-first delivery milestones.', 'Checkout prototype ready for review', 'In Progress', 48, 'High', '2026-07-31T00:00:00Z', 1, '2026-07-25T08:00:00Z'),
('expert@example.com', 'Local Healthcare Data Dashboard', 'Jade Taylor', 'Analytics', 'Create a lightweight dashboard for practice metrics and reporting visibility.', 'Data schema alignment', 'Ready for build', 22, 'Medium', '2026-08-05T00:00:00Z', 1, '2026-07-25T03:00:00Z');

INSERT INTO DeliveryLogs (DeliveryAssignmentId, ExpertEmail, CreatedByEmail, Level, Message, CreatedAt)
VALUES
(1, 'expert@example.com', 'expert@example.com', 'info', 'Completed first pass on mobile layouts and core checkout states.', '2026-07-25T04:00:00Z'),
(1, 'expert@example.com', 'expert@example.com', 'success', 'Shared milestone preview with the SME for feedback.', '2026-07-25T06:00:00Z'),
(2, 'expert@example.com', 'expert@example.com', 'warning', 'Waiting on data sample confirmation before building dashboard cards.', '2026-07-25T07:00:00Z');

INSERT INTO EscrowOnboardingProfiles (ExpertEmail, ProviderAccountId, Status, OnboardingUrl, LastSyncedAtUtc, OnboardedAtUtc)
VALUES
('expert@example.com', 'pinch-glassbox-expert-example-com', 'Pending', 'https://connect.getpinch.com.au/glassbox/onboarding/pinch-glassbox-expert-example-com', '2026-07-25T08:00:00Z', NULL);

INSERT INTO ProjectPaymentStateRecords (ProjectId, State, GrossAmount, PlatformFeeAmount, ContractorAmount, Currency, LastProviderReference, UpdatedAtUtc)
VALUES
('demo-project-epic7-1', 'AwaitingPayment', 9500, 0, 0, 'AUD', NULL, '2026-07-25T08:00:00Z'),
('demo-project-epic7-2', 'AwaitingPayment', 14500, 0, 0, 'AUD', NULL, '2026-07-25T08:00:00Z');

INSERT INTO Users (Email, PasswordHash, Role, CreatedAt, IsActive, CompanyName, BusinessIdentifier, ProfileSummary, OnboardingCompleted, OnboardedAt)
VALUES
('sme@example.com', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'SME', '2026-07-01T00:00:00Z', 1, 'Coastal SME Services', 'ABN 12 345 678 901', 'Regional digital transformation for small businesses.', 1, '2026-07-01T00:00:00Z'),
('expert@example.com', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Expert', '2026-07-01T00:00:00Z', 1, 'North Shore Dev Studio', 'ABN 98 765 432 109', 'Freelance expert in compliance-first application delivery.', 1, '2026-07-01T00:00:00Z'),
('admin@example.com', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Admin', '2026-07-01T00:00:00Z', 1, 'Symbio Platform Admin', 'ABN 00 000 000 000', 'Platform administrator with full system oversight.', 1, '2026-07-01T00:00:00Z');

INSERT INTO AdminUserComplianceRecords (UserEmail, UserRole, ReviewStatus, RiskLevel, Notes, ReviewedByEmail, CreatedAtUtc, ReviewedAtUtc)
VALUES
('expert@example.com', 'Expert', 'Pending', 'Medium', 'Profile requires quarterly trust audit refresh.', NULL, '2026-07-23T00:00:00Z', NULL),
('sme@example.com', 'SME', 'Pending', 'Low', 'ABN confirmation scheduled for next cycle.', NULL, '2026-07-24T00:00:00Z', NULL);

INSERT INTO AdminProjectFlagRecords (ProjectId, MilestoneId, ReportedByEmail, Severity, Reason, Status, ResolvedByEmail, CreatedAtUtc, ResolvedAtUtc)
VALUES
('demo-project-epic7-1', 'Kickoff', 'system@symbio.local', 'High', 'Settlement latency exceeded threshold and requires manual review.', 'Open', NULL, '2026-07-25T02:00:00Z', NULL);

INSERT INTO AdminSafetySettings (SettingKey, SettingValue, UpdatedByEmail, UpdatedAtUtc)
VALUES
('payments.settlement.autoReleaseEnabled', 'false', 'admin@example.com', '2026-07-24T20:00:00Z'),
('compliance.maxOpenFlagsBeforeEscalation', '5', 'admin@example.com', '2026-07-25T00:00:00Z');
