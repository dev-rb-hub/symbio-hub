using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Hubs;
using Symbio.API.Models;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/expert")]
    [Authorize(Roles = "Expert")]
    public class ExpertWorkbenchController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;
        private readonly IHubContext<DeliveryWorkbenchHub> _hubContext;
        private readonly ICompletionEvidenceRepository _completionEvidenceRepository;

        public ExpertWorkbenchController(
            SymbioDbContext dbContext,
            IHubContext<DeliveryWorkbenchHub> hubContext,
            ICompletionEvidenceRepository completionEvidenceRepository)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _completionEvidenceRepository = completionEvidenceRepository;
        }

        public record WorkbenchLogRequest(
            int DeliveryAssignmentId,
            string Message,
            string? Level,
            int? ProgressPercent,
            string? Status,
            string? MilestoneId,
            string? EpicId,
            string? EvidenceType,
            string? EvidenceReferenceValue,
            string? TargetDeploymentUrl,
            string? SourceCommitSha,
            string? EvidenceNotes);

        public sealed record ExpertDashboardQuery(
            string? Search,
            string? Status,
            string? Priority,
            int? ReportLimit);

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] ExpertDashboardQuery query)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var assignmentsQuery = _dbContext.DeliveryAssignments
                .Where(item => item.ExpertEmail == email && item.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var statusFilter = query.Status.Trim();
                assignmentsQuery = assignmentsQuery.Where(item => item.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(query.Priority))
            {
                var priorityFilter = query.Priority.Trim();
                assignmentsQuery = assignmentsQuery.Where(item => item.Priority == priorityFilter);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLowerInvariant();
                assignmentsQuery = assignmentsQuery.Where(item =>
                    item.ProjectTitle.ToLower().Contains(search)
                    || item.ClientName.ToLower().Contains(search)
                    || item.CurrentMilestone.ToLower().Contains(search)
                    || item.Category.ToLower().Contains(search));
            }

            var assignments = await assignmentsQuery
                .OrderBy(item => item.DueDate)
                .ToListAsync();

            var assignmentIds = assignments.Select(item => item.Id).ToList();
            var reportLimit = query.ReportLimit.GetValueOrDefault(40);
            if (reportLimit <= 0)
            {
                reportLimit = 40;
            }

            reportLimit = Math.Min(reportLimit, 200);

            var logs = assignmentIds.Count == 0
                ? new List<DeliveryLogEntry>()
                : await _dbContext.DeliveryLogs
                    .Where(log => assignmentIds.Contains(log.DeliveryAssignmentId))
                    .OrderByDescending(log => log.CreatedAt)
                    .Take(reportLimit)
                    .ToListAsync();

            var assignmentMap = assignments.ToDictionary(item => item.Id, item => item);
            var escrowProfile = await _dbContext.EscrowOnboardingProfiles.FirstOrDefaultAsync(item => item.ExpertEmail == email);
            var escrowVerified = escrowProfile != null
                && escrowProfile.Status.Equals(EscrowOnboardingStatus.Verified.ToString(), StringComparison.OrdinalIgnoreCase);

            var evidenceMatrix = await _completionEvidenceRepository.GetMatrixAsync();
            var expertEvidence = evidenceMatrix.Records
                .Where(item => item.LoggedByEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var projects = assignments
                .GroupBy(item => item.ProjectTitle)
                .Select(group => new
                {
                    projectTitle = group.Key,
                    clientName = group.First().ClientName,
                    category = group.First().Category,
                    assignmentCount = group.Count(),
                    progressAverage = (int)Math.Round(group.Average(item => item.ProgressPercent)),
                    dueSoonest = group.Min(item => item.DueDate),
                    statuses = group.Select(item => item.Status).Distinct().OrderBy(item => item).ToArray(),
                })
                .OrderBy(item => item.dueSoonest)
                .ToList();

            var milestones = assignments
                .GroupBy(item => new { item.ProjectTitle, item.CurrentMilestone })
                .Select(group => new
                {
                    projectTitle = group.Key.ProjectTitle,
                    milestone = group.Key.CurrentMilestone,
                    status = group.OrderByDescending(item => item.UpdatedAt).First().Status,
                    progressAverage = (int)Math.Round(group.Average(item => item.ProgressPercent)),
                    priority = group.OrderByDescending(item => item.UpdatedAt).First().Priority,
                    dueDate = group.Min(item => item.DueDate),
                    assignmentCount = group.Count(),
                })
                .OrderBy(item => item.dueDate)
                .ToList();

            var evidenceByMilestone = expertEvidence
                .GroupBy(item => item.MilestoneId)
                .ToDictionary(item => item.Key, item => item.ToList());

            var payments = milestones
                .Select(item =>
                {
                    var evidence = evidenceByMilestone.TryGetValue(item.milestone, out var records) ? records : new List<CompletionEvidenceRecord>();
                    var evidenceCount = evidence.Count;
                    var verifiedEvidenceCount = evidence.Count(record => record.IsVerified);
                    var paymentState = !escrowVerified
                        ? "EscrowPending"
                        : verifiedEvidenceCount > 0
                            ? "SettlementReady"
                            : "EvidencePending";

                    return new
                    {
                        item.projectTitle,
                        milestone = item.milestone,
                        paymentState,
                        escrowVerified,
                        evidenceCount,
                        verifiedEvidenceCount,
                        lastEvidenceAtUtc = evidence.OrderByDescending(record => record.LoggedAtUtc).FirstOrDefault()?.LoggedAtUtc,
                    };
                })
                .OrderBy(item => item.projectTitle)
                .ThenBy(item => item.milestone)
                .ToList();

            var reports = logs
                .Select(log =>
                {
                    assignmentMap.TryGetValue(log.DeliveryAssignmentId, out var assignment);

                    return new
                    {
                        log.Id,
                        log.DeliveryAssignmentId,
                        projectTitle = assignment?.ProjectTitle ?? "Unknown project",
                        currentMilestone = assignment?.CurrentMilestone ?? string.Empty,
                        log.Level,
                        log.Message,
                        log.CreatedAt,
                    };
                })
                .ToList();

            var reportLevelSummary = reports
                .GroupBy(item => item.Level)
                .Select(group => new
                {
                    level = group.Key,
                    count = group.Count(),
                })
                .OrderByDescending(item => item.count)
                .ToList();

            return Ok(new
            {
                expertEmail = email,
                filters = new
                {
                    query.Search,
                    query.Status,
                    query.Priority,
                    reportLimit,
                },
                totals = new
                {
                    projectCount = projects.Count,
                    milestoneCount = milestones.Count,
                    paymentItemCount = payments.Count,
                    reportCount = reports.Count,
                },
                projects,
                milestones,
                payments,
                reports,
                reportLevelSummary,
                escrow = new
                {
                    status = escrowProfile?.Status ?? EscrowOnboardingStatus.NotStarted.ToString(),
                    escrowVerified,
                    providerAccountId = escrowProfile?.ProviderAccountId ?? string.Empty,
                }
            });
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var assignments = await _dbContext.DeliveryAssignments
                .Where(item => item.ExpertEmail == email && item.IsActive)
                .OrderBy(item => item.DueDate)
                .ToListAsync();

            var assignmentIds = assignments.Select(item => item.Id).ToList();
            var recentLogs = await _dbContext.DeliveryLogs
                .Where(log => assignmentIds.Contains(log.DeliveryAssignmentId))
                .OrderByDescending(log => log.CreatedAt)
                .Take(20)
                .ToListAsync();

            return Ok(new
            {
                expertEmail = email,
                assignments,
                recentLogs
            });
        }

        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog([FromBody] WorkbenchLogRequest request)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "A log message is required." });
            }

            var statusNormalized = request.Status?.Trim().ToLowerInvariant() ?? string.Empty;
            var statusSignalsSettlement = statusNormalized.Contains("settle")
                || statusNormalized.Contains("complete")
                || statusNormalized.Contains("done");

            var hasMilestoneContext = !string.IsNullOrWhiteSpace(request.MilestoneId);
            var hasEvidencePayload = !string.IsNullOrWhiteSpace(request.EvidenceType)
                && !string.IsNullOrWhiteSpace(request.EvidenceReferenceValue);

            var requiresEscrowVerification = hasMilestoneContext
                || hasEvidencePayload
                || statusSignalsSettlement
                || request.ProgressPercent.GetValueOrDefault() >= 100;

            if (requiresEscrowVerification)
            {
                var escrowProfile = await _dbContext.EscrowOnboardingProfiles
                    .FirstOrDefaultAsync(item => item.ExpertEmail == email);

                var escrowVerified = escrowProfile != null
                    && escrowProfile.Status.Equals(EscrowOnboardingStatus.Verified.ToString(), StringComparison.OrdinalIgnoreCase);

                if (!escrowVerified)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, new
                    {
                        message = "Escrow onboarding must be verified before posting milestone completion, settlement, or evidence-related updates."
                    });
                }
            }

            var assignment = await _dbContext.DeliveryAssignments.FirstOrDefaultAsync(item => item.Id == request.DeliveryAssignmentId && item.ExpertEmail == email && item.IsActive);
            if (assignment == null)
            {
                return NotFound(new { message = "Delivery assignment not found." });
            }

            if (assignment.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = "This assignment is currently under review and locked for expert edits." });
            }

            if (request.ProgressPercent.HasValue)
            {
                assignment.ProgressPercent = Math.Clamp(request.ProgressPercent.Value, 0, 100);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                assignment.Status = request.Status.Trim();
            }

            assignment.UpdatedAt = DateTime.UtcNow;

            var log = new DeliveryLogEntry
            {
                DeliveryAssignmentId = assignment.Id,
                ExpertEmail = email,
                CreatedByEmail = email,
                Level = string.IsNullOrWhiteSpace(request.Level) ? "info" : request.Level.Trim().ToLowerInvariant(),
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.DeliveryLogs.Add(log);

            var shouldCaptureEvidence = !string.IsNullOrWhiteSpace(request.MilestoneId)
                && !string.IsNullOrWhiteSpace(request.EvidenceType)
                && !string.IsNullOrWhiteSpace(request.EvidenceReferenceValue);

            if (shouldCaptureEvidence)
            {
                if (!TryParseEvidenceType(request.EvidenceType!, out var parsedType))
                {
                    return BadRequest(new { message = "EvidenceType must be BuildArtifactHash or GitCommitSHA." });
                }

                CompletionEvidenceRecord record;
                if (parsedType == CompletionEvidenceType.BuildArtifactHash)
                {
                    record = CompletionEvidenceRecord.FromArtifactHash(
                        request.MilestoneId!,
                        request.EpicId ?? string.Empty,
                        request.EvidenceReferenceValue!,
                        email,
                        request.TargetDeploymentUrl ?? string.Empty,
                        request.SourceCommitSha,
                        request.EvidenceNotes);
                }
                else
                {
                    record = CompletionEvidenceRecord.FromGitCommit(
                        request.MilestoneId!,
                        request.EpicId ?? string.Empty,
                        request.EvidenceReferenceValue!,
                        email,
                        request.TargetDeploymentUrl ?? string.Empty,
                        request.SourceCommitSha,
                        request.EvidenceNotes);
                }

                await _completionEvidenceRepository.RecordAsync(record);
            }

            await _dbContext.SaveChangesAsync();

            var payload = new
            {
                log.Id,
                log.DeliveryAssignmentId,
                log.ExpertEmail,
                log.CreatedByEmail,
                log.Level,
                log.Message,
                log.CreatedAt,
                assignment.ProgressPercent,
                assignment.Status,
                assignment.CurrentMilestone,
                assignment.ProjectTitle
            };

            await _hubContext.Clients.Group(DeliveryWorkbenchHub.GetGroupName(email)).SendAsync("WorkbenchLogCreated", payload);

            return Ok(payload);
        }

        private static bool TryParseEvidenceType(string value, out CompletionEvidenceType evidenceType)
        {
            evidenceType = CompletionEvidenceType.BuildArtifactHash;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim();
            if (normalized.Equals("BuildArtifactHash", StringComparison.OrdinalIgnoreCase))
            {
                evidenceType = CompletionEvidenceType.BuildArtifactHash;
                return true;
            }

            if (normalized.Equals("GitCommitSHA", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("GitCommitSha", StringComparison.OrdinalIgnoreCase))
            {
                evidenceType = CompletionEvidenceType.GitCommitSha;
                return true;
            }

            return false;
        }
    }
}