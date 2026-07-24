using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Models;

namespace Symbio.API.Endpoints;

public static class AdminOperationsEndpoints
{
    private const string AdminMasterClaimType = "symbio_admin_master";

    public sealed record CreateProjectFlagRequest(string ProjectId, string MilestoneId, string Severity, string Reason, string? ReportedByEmail);
    public sealed record ResolveComplianceReviewRequest(string ResolutionNotes);
    public sealed record UpsertSafetySettingRequest(string SettingKey, string SettingValue);
    public sealed record UserActivationOverrideRequest(bool IsActive, string Reason);

    public static IEndpointRouteBuilder MapAdminOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin")
            .RequireAuthorization("RequireAdminRole");

        admin.MapGet("/telemetry/global", GetGlobalTelemetry);
        admin.MapGet("/compliance/queue", GetComplianceQueue);
        admin.MapPost("/compliance/flags", CreateProjectFlag);
        admin.MapPost("/compliance/reviews/{reviewId:int}/resolve", ResolveComplianceReview);
        admin.MapGet("/overrides/safety-settings", GetSafetySettings);
        admin.MapPost("/overrides/safety-settings", UpsertSafetySetting);
        admin.MapPost("/overrides/users/{userId:int}/activation", SetUserActivation);

        return app;
    }

    private static async Task<IResult> GetGlobalTelemetry(HttpContext context, SymbioDbContext dbContext, IConfiguration configuration)
    {
        var users = await dbContext.Users.AsNoTracking().ToListAsync();
        var expertCount = users.Count(item => item.Role == "Expert");
        var smeCount = users.Count(item => item.Role == "SME");
        var adminCount = users.Count(item => item.Role == "Admin");

        var onboardedCount = users.Count(item => item.OnboardingCompleted);
        var escrowProfiles = await dbContext.EscrowOnboardingProfiles.AsNoTracking().ToListAsync();
        var escrowVerifiedCount = escrowProfiles.Count(item => string.Equals(item.Status, "Verified", StringComparison.OrdinalIgnoreCase));

        var pendingCompliance = await dbContext.AdminUserComplianceRecords
            .AsNoTracking()
            .CountAsync(item => item.ReviewStatus == "Pending");

        var openFlags = await dbContext.AdminProjectFlagRecords
            .AsNoTracking()
            .CountAsync(item => item.Status == "Open");

        var storage = BuildStorageVolumeTelemetry(dbContext, configuration);

        var response = new
        {
            generatedAtUtc = DateTime.UtcNow,
            storage,
            userProfileHealth = new
            {
                totalUsers = users.Count,
                smeCount,
                expertCount,
                adminCount,
                onboardedCount,
                onboardingCompletionRate = users.Count == 0 ? 0m : Math.Round((decimal)onboardedCount / users.Count, 4),
                escrowVerifiedCount,
                escrowVerificationRate = expertCount == 0 ? 0m : Math.Round((decimal)escrowVerifiedCount / expertCount, 4)
            },
            regionalProfileHealth = new[]
            {
                new
                {
                    region = "ANZ",
                    activeProfiles = users.Count(item => item.IsActive),
                    pendingCompliance,
                    openFlags,
                    healthScore = Math.Max(0m, 1m - (pendingCompliance + openFlags) / Math.Max(1m, (decimal)users.Count))
                }
            }
        };

        return Results.Ok(response);
    }

    private static object BuildStorageVolumeTelemetry(SymbioDbContext dbContext, IConfiguration configuration)
    {
        var connectionString = dbContext.Database.IsRelational()
            ? dbContext.Database.GetConnectionString() ?? configuration.GetConnectionString("DefaultConnection") ?? string.Empty
            : configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        long? sqliteFileBytes = null;

        var dataSource = ParseSqliteDataSource(connectionString);
        if (!string.IsNullOrWhiteSpace(dataSource))
        {
            var absolutePath = Path.IsPathRooted(dataSource)
                ? dataSource
                : Path.Combine(AppContext.BaseDirectory, dataSource);

            if (File.Exists(absolutePath))
            {
                sqliteFileBytes = new FileInfo(absolutePath).Length;
            }
        }

        return new
        {
            databaseProvider = dbContext.Database.ProviderName ?? "unknown",
            sqliteFileBytes,
            tableRowVolumes = new
            {
                users = dbContext.Users.Count(),
                jobs = dbContext.Jobs.Count(),
                deliveryAssignments = dbContext.DeliveryAssignments.Count(),
                paymentStates = dbContext.ProjectPaymentStateRecords.Count(),
                invoices = dbContext.AccountingInvoices.Count(),
                retainerContracts = dbContext.RetainerContracts.Count(),
                complianceReviews = dbContext.AdminUserComplianceRecords.Count(),
                projectFlags = dbContext.AdminProjectFlagRecords.Count(),
                auditLogs = dbContext.AdminAuditLogs.Count()
            }
        };
    }

    private static string ParseSqliteDataSource(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        var segments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            if (segment.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                return segment["Data Source=".Length..].Trim();
            }
        }

        return string.Empty;
    }

    private static async Task<IResult> GetComplianceQueue(SymbioDbContext dbContext)
    {
        var pendingReviews = await dbContext.AdminUserComplianceRecords
            .AsNoTracking()
            .Where(item => item.ReviewStatus == "Pending")
            .OrderBy(item => item.CreatedAtUtc)
            .ToListAsync();

        var openFlags = await dbContext.AdminProjectFlagRecords
            .AsNoTracking()
            .Where(item => item.Status == "Open")
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(100)
            .ToListAsync();

        return Results.Ok(new
        {
            pendingReviewCount = pendingReviews.Count,
            openFlagCount = openFlags.Count,
            pendingReviews,
            openFlags
        });
    }

    private static async Task<IResult> CreateProjectFlag(HttpContext context, SymbioDbContext dbContext, [FromBody] CreateProjectFlagRequest request)
    {
        if (request == null
            || string.IsNullOrWhiteSpace(request.ProjectId)
            || string.IsNullOrWhiteSpace(request.MilestoneId)
            || string.IsNullOrWhiteSpace(request.Reason))
        {
            return Results.BadRequest(new { message = "ProjectId, MilestoneId and Reason are required." });
        }

        var adminEmail = context.User.Identity?.Name ?? "admin@unknown";
        var flag = new AdminProjectFlagRecord
        {
            ProjectId = request.ProjectId.Trim(),
            MilestoneId = request.MilestoneId.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "Medium" : request.Severity.Trim(),
            Reason = request.Reason.Trim(),
            ReportedByEmail = string.IsNullOrWhiteSpace(request.ReportedByEmail) ? adminEmail : request.ReportedByEmail.Trim(),
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.AdminProjectFlagRecords.Add(flag);
        dbContext.AdminAuditLogs.Add(CreateAuditLog(adminEmail, "CreateProjectFlag", "ProjectFlag", $"{flag.ProjectId}:{flag.MilestoneId}", new
        {
            flag.Severity,
            flag.Reason
        }));

        await dbContext.SaveChangesAsync();

        return Results.Ok(flag);
    }

    private static async Task<IResult> ResolveComplianceReview(HttpContext context, SymbioDbContext dbContext, int reviewId, [FromBody] ResolveComplianceReviewRequest request)
    {
        var review = await dbContext.AdminUserComplianceRecords.FirstOrDefaultAsync(item => item.Id == reviewId);
        if (review == null)
        {
            return Results.NotFound(new { message = "Compliance review not found." });
        }

        var adminEmail = context.User.Identity?.Name ?? "admin@unknown";
        review.ReviewStatus = "Resolved";
        review.ReviewedByEmail = adminEmail;
        review.ReviewedAtUtc = DateTime.UtcNow;
        if (request != null && !string.IsNullOrWhiteSpace(request.ResolutionNotes))
        {
            review.Notes = request.ResolutionNotes.Trim();
        }

        dbContext.AdminAuditLogs.Add(CreateAuditLog(adminEmail, "ResolveComplianceReview", "ComplianceReview", review.Id.ToString(), new
        {
            review.UserEmail,
            review.RiskLevel,
            review.Notes
        }));

        await dbContext.SaveChangesAsync();

        return Results.Ok(review);
    }

    private static async Task<IResult> GetSafetySettings(SymbioDbContext dbContext)
    {
        var settings = await dbContext.AdminSafetySettings
            .AsNoTracking()
            .OrderBy(item => item.SettingKey)
            .ToListAsync();

        return Results.Ok(settings);
    }

    private static async Task<IResult> UpsertSafetySetting(HttpContext context, SymbioDbContext dbContext, [FromBody] UpsertSafetySettingRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SettingKey))
        {
            return Results.BadRequest(new { message = "SettingKey is required." });
        }

        var adminEmail = context.User.Identity?.Name ?? "admin@unknown";
        var key = request.SettingKey.Trim();
        var value = request.SettingValue?.Trim() ?? string.Empty;

        var setting = await dbContext.AdminSafetySettings
            .FirstOrDefaultAsync(item => item.SettingKey == key);

        if (setting == null)
        {
            setting = new AdminSafetySettingRecord
            {
                SettingKey = key,
                SettingValue = value,
                UpdatedByEmail = adminEmail,
                UpdatedAtUtc = DateTime.UtcNow
            };
            dbContext.AdminSafetySettings.Add(setting);
        }
        else
        {
            setting.SettingValue = value;
            setting.UpdatedByEmail = adminEmail;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }

        dbContext.AdminAuditLogs.Add(CreateAuditLog(adminEmail, "UpsertSafetySetting", "SafetySetting", key, new
        {
            setting.SettingValue
        }));

        await dbContext.SaveChangesAsync();

        return Results.Ok(setting);
    }

    private static async Task<IResult> SetUserActivation(HttpContext context, SymbioDbContext dbContext, int userId, [FromBody] UserActivationOverrideRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user == null)
        {
            return Results.NotFound(new { message = "User not found." });
        }

        var adminEmail = context.User.Identity?.Name ?? "admin@unknown";
        user.IsActive = request.IsActive;

        dbContext.AdminAuditLogs.Add(CreateAuditLog(adminEmail, "SetUserActivation", "User", user.Id.ToString(), new
        {
            user.Email,
            user.IsActive,
            reason = request.Reason
        }));

        await dbContext.SaveChangesAsync();

        return Results.Ok(new
        {
            user.Id,
            user.Email,
            user.IsActive
        });
    }

    private static AdminAuditLogRecord CreateAuditLog(string adminEmail, string action, string targetType, string targetReference, object detail)
    {
        return new AdminAuditLogRecord
        {
            AdminEmail = adminEmail,
            Action = action,
            TargetType = targetType,
            TargetReference = targetReference,
            DetailJson = JsonSerializer.Serialize(detail),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
