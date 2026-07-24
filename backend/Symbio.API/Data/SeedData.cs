using Microsoft.EntityFrameworkCore;
using Symbio.API.Models;
using Symbio.Core.Models;

namespace Symbio.API.Data
{
    public static class SeedData
    {
        public static void Initialize(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                var users = new[]
                {
                    new User
                    {
                        Email = "sme@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "SME",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        CompanyName = "Coastal SME Services",
                        BusinessIdentifier = "ABN 12 345 678 901",
                        ProfileSummary = "Regional digital transformation for small businesses.",
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "expert@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "Expert",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        CompanyName = "North Shore Dev Studio",
                        BusinessIdentifier = "ABN 98 765 432 109",
                        ProfileSummary = "Freelance expert in compliance-first application delivery.",
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "admin@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "Admin",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        CompanyName = "Symbio Platform Admin",
                        BusinessIdentifier = "ABN 00 000 000 000",
                        ProfileSummary = "Platform administrator with full system oversight.",
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    }
                };

                db.Users.AddRange(users);
                db.SaveChanges();
            }

            if (db.Jobs.Any())
            {
                // keep going so later bootstrap data can be added independently
            }

            if (!db.Jobs.Any())
            {
                var jobs = new[]
                {
                    new Job
                    {
                        Title = "Regional Retail Website Refresh",
                        Description = "Build a mobile-first homepage and checkout experience for a small NSW retail brand.",
                        ClientName = "Harper",
                        ClientSurname = "Bright",
                        Budget = 9500m,
                        ContactEmail = "contact@harperbright.com",
                        IsPublished = true,
                        PostedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Job
                    {
                        Title = "Local Healthcare Data Dashboard",
                        Description = "Create a lightweight reporting dashboard for a regional practice using anonymised patient metrics.",
                        ClientName = "Jade",
                        ClientSurname = "Taylor",
                        Budget = 14500m,
                        ContactEmail = "jade.taylor@coastalhealth.au",
                        IsPublished = true,
                        PostedAt = DateTime.UtcNow.AddDays(-12)
                    },
                    new Job
                    {
                        Title = "Food Delivery Loyalty Campaign",
                        Description = "Design and build a customer loyalty landing page with signup flow and campaign analytics.",
                        ClientName = "Miles",
                        ClientSurname = "Kerr",
                        Budget = 7200m,
                        ContactEmail = "miles@harvestdeli.au",
                        IsPublished = true,
                        PostedAt = DateTime.UtcNow.AddDays(-2)
                    }
                };

                db.Jobs.AddRange(jobs);
                db.SaveChanges();
            }

            if (db.Database.IsRelational())
            {
                db.Database.ExecuteSqlRaw(@"
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
");
            }

            if (!db.DeliveryAssignments.Any())
            {
                var assignments = new[]
                {
                    new DeliveryAssignment
                    {
                        Id = 1,
                        ExpertEmail = "expert@example.com",
                        ProjectTitle = "Regional Retail Website Refresh",
                        ClientName = "Harper Bright",
                        Category = "Web Experience",
                        ScopeSummary = "Rebuild the public homepage and checkout flow with mobile-first delivery milestones.",
                        CurrentMilestone = "Checkout prototype ready for review",
                        Status = "In Progress",
                        ProgressPercent = 48,
                        Priority = "High",
                        DueDate = DateTime.UtcNow.AddDays(7),
                        IsActive = true,
                        UpdatedAt = DateTime.UtcNow.AddHours(-3)
                    },
                    new DeliveryAssignment
                    {
                        Id = 2,
                        ExpertEmail = "expert@example.com",
                        ProjectTitle = "Local Healthcare Data Dashboard",
                        ClientName = "Jade Taylor",
                        Category = "Analytics",
                        ScopeSummary = "Create a lightweight dashboard for practice metrics and reporting visibility.",
                        CurrentMilestone = "Data schema alignment",
                        Status = "Ready for build",
                        ProgressPercent = 22,
                        Priority = "Medium",
                        DueDate = DateTime.UtcNow.AddDays(12),
                        IsActive = true,
                        UpdatedAt = DateTime.UtcNow.AddHours(-8)
                    }
                };

                db.DeliveryAssignments.AddRange(assignments);
                db.SaveChanges();
            }

            if (!db.DeliveryLogs.Any())
            {
                var logs = new[]
                {
                    new DeliveryLogEntry
                    {
                        DeliveryAssignmentId = 1,
                        ExpertEmail = "expert@example.com",
                        CreatedByEmail = "expert@example.com",
                        Level = "info",
                        Message = "Completed first pass on mobile layouts and core checkout states.",
                        CreatedAt = DateTime.UtcNow.AddHours(-4)
                    },
                    new DeliveryLogEntry
                    {
                        DeliveryAssignmentId = 1,
                        ExpertEmail = "expert@example.com",
                        CreatedByEmail = "expert@example.com",
                        Level = "success",
                        Message = "Shared milestone preview with the SME for feedback.",
                        CreatedAt = DateTime.UtcNow.AddHours(-2)
                    },
                    new DeliveryLogEntry
                    {
                        DeliveryAssignmentId = 2,
                        ExpertEmail = "expert@example.com",
                        CreatedByEmail = "expert@example.com",
                        Level = "warning",
                        Message = "Waiting on data sample confirmation before building dashboard cards.",
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    }
                };

                db.DeliveryLogs.AddRange(logs);
                db.SaveChanges();
            }

            if (!db.EscrowOnboardingProfiles.Any())
            {
                var onboardingProfiles = new[]
                {
                    new EscrowOnboardingProfile
                    {
                        ExpertEmail = "expert@example.com",
                        ProviderAccountId = "pinch-glassbox-expert-example-com",
                        Status = EscrowOnboardingStatus.Pending.ToString(),
                        OnboardingUrl = "https://connect.getpinch.com.au/glassbox/onboarding/pinch-glassbox-expert-example-com",
                        LastSyncedAtUtc = DateTime.UtcNow,
                        OnboardedAtUtc = null
                    }
                };

                db.EscrowOnboardingProfiles.AddRange(onboardingProfiles);
                db.SaveChanges();
            }

            if (!db.ProjectPaymentStateRecords.Any())
            {
                var paymentStates = new[]
                {
                    new ProjectPaymentStateRecord
                    {
                        ProjectId = "demo-project-epic7-1",
                        State = "AwaitingPayment",
                        GrossAmount = 9500m,
                        PlatformFeeAmount = 0m,
                        ContractorAmount = 0m,
                        Currency = "AUD",
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    new ProjectPaymentStateRecord
                    {
                        ProjectId = "demo-project-epic7-2",
                        State = "AwaitingPayment",
                        GrossAmount = 14500m,
                        PlatformFeeAmount = 0m,
                        ContractorAmount = 0m,
                        Currency = "AUD",
                        UpdatedAtUtc = DateTime.UtcNow
                    }
                };

                db.ProjectPaymentStateRecords.AddRange(paymentStates);
                db.SaveChanges();
            }
        }

        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password ?? string.Empty);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
