using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Models;

namespace Symbio.API.Controllers;

[ApiController]
[Route("api/agreements")]
public class AgreementsController : ControllerBase
{
    private static class AgreementStatuses
    {
        public const string PendingApproval = "PendingApproval";
        public const string Active = "Active";
        public const string Closed = "Closed";
    }

    public sealed record UpsertAgreementRequest(
        string ProjectId,
        string? ProjectTitle,
        string? MilestoneId,
        decimal? Amount,
        string? Currency,
        int? SmeUserId,
        int? ExpertUserId,
        string? ExpertEmail,
        string? Status);

    public sealed record ApproveAgreementRequest(string? TargetRole);

    public sealed record UpdateAgreementStatusRequest(string Status);

    private readonly SymbioDbContext _dbContext;

    public AgreementsController(SymbioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "SME,Expert,Admin")]
    public async Task<IActionResult> GetAgreements([FromQuery] string? search, [FromQuery] bool includePending = false, [FromQuery] bool includeClosed = false)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var query = _dbContext.Agreements
            .AsNoTracking()
            .AsQueryable();

        if (string.Equals(currentUser.Role, "SME", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(item => item.SmeUserId == currentUser.Id || item.SmeEmail == currentUser.Email);
        }
        else if (string.Equals(currentUser.Role, "Expert", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(item => item.ExpertUserId == currentUser.Id || item.ExpertEmail == currentUser.Email);
        }

        if (!includePending)
        {
            query = query.Where(item => item.Status != AgreementStatuses.PendingApproval);
        }

        if (!includeClosed)
        {
            query = query.Where(item => item.Status != AgreementStatuses.Closed);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item =>
                item.ProjectId.Contains(term)
                || item.ProjectTitle.Contains(term)
                || item.MilestoneId.Contains(term)
                || item.SmeEmail.Contains(term)
                || (item.ExpertEmail != null && item.ExpertEmail.Contains(term))
                || item.Status.Contains(term));
        }

        var results = await query
            .OrderByDescending(item => item.UpdatedAtUtc)
            .Take(250)
            .Select(item => new
            {
                item.Id,
                item.ProjectId,
                item.ProjectTitle,
                item.MilestoneId,
                item.SmeUserId,
                item.ExpertUserId,
                talentUserId = item.ExpertUserId,
                item.SmeEmail,
                item.ExpertEmail,
                item.Amount,
                item.Currency,
                item.Status,
                item.SmeApprovedAtUtc,
                item.ExpertApprovedAtUtc,
                item.ClosedAtUtc,
                item.UpdatedAtUtc,
                isCurrentUserProjectOwner = item.SmeUserId == currentUser.Id,
                isCurrentUserTalent = item.ExpertUserId == currentUser.Id
            })
            .ToListAsync();

        return Ok(new
        {
            count = results.Count,
            agreements = results
        });
    }

    [HttpPost("upsert")]
    [Authorize(Roles = "SME,Expert,Admin")]
    public async Task<IActionResult> UpsertAgreement([FromBody] UpsertAgreementRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ProjectId))
        {
            return BadRequest(new { message = "ProjectId is required." });
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var projectId = request.ProjectId.Trim();
        var agreement = await _dbContext.Agreements
            .FirstOrDefaultAsync(item => item.ProjectId == projectId);

        if (agreement == null)
        {
            agreement = new AgreementRecord
            {
                ProjectId = projectId,
                Status = AgreementStatuses.PendingApproval,
            };
            _dbContext.Agreements.Add(agreement);
        }

        agreement.ProjectTitle = string.IsNullOrWhiteSpace(request.ProjectTitle)
            ? agreement.ProjectTitle
            : request.ProjectTitle.Trim();
        agreement.MilestoneId = string.IsNullOrWhiteSpace(request.MilestoneId)
            ? (string.IsNullOrWhiteSpace(agreement.MilestoneId) ? "Kickoff" : agreement.MilestoneId)
            : request.MilestoneId.Trim();
        agreement.Amount = request.Amount.GetValueOrDefault(agreement.Amount);
        agreement.Currency = string.IsNullOrWhiteSpace(request.Currency)
            ? (string.IsNullOrWhiteSpace(agreement.Currency) ? "AUD" : agreement.Currency)
            : request.Currency.Trim().ToUpperInvariant();

        if (string.Equals(currentUser.Role, "SME", StringComparison.OrdinalIgnoreCase))
        {
            agreement.SmeUserId = currentUser.Id;
            agreement.SmeEmail = currentUser.Email;
        }
        else if (request.SmeUserId.HasValue)
        {
            var sme = await _dbContext.Users
                .FirstOrDefaultAsync(item => item.Id == request.SmeUserId.Value && item.IsActive);
            if (sme != null)
            {
                agreement.SmeUserId = sme.Id;
                agreement.SmeEmail = sme.Email;
            }
        }

        if (string.Equals(currentUser.Role, "Expert", StringComparison.OrdinalIgnoreCase))
        {
            agreement.ExpertUserId = currentUser.Id;
            agreement.ExpertEmail = currentUser.Email;
        }
        else
        {
            if (request.ExpertUserId.HasValue)
            {
                var expertById = await _dbContext.Users
                    .FirstOrDefaultAsync(item => item.Id == request.ExpertUserId.Value && item.IsActive);
                if (expertById != null)
                {
                    agreement.ExpertUserId = expertById.Id;
                    agreement.ExpertEmail = expertById.Email;
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.ExpertEmail))
            {
                var normalizedEmail = request.ExpertEmail.Trim().ToLowerInvariant();
                var expertByEmail = await _dbContext.Users
                    .FirstOrDefaultAsync(item => item.Email == normalizedEmail && item.IsActive);

                agreement.ExpertUserId = expertByEmail?.Id;
                agreement.ExpertEmail = normalizedEmail;
            }
        }

        if (agreement.SmeUserId <= 0)
        {
            return BadRequest(new { message = "A valid SME relationship is required for each agreement record." });
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var normalizedStatus = NormalizeStatus(request.Status);
            if (normalizedStatus == null)
            {
                return BadRequest(new { message = "Status must be PendingApproval, Active, or Closed." });
            }

            agreement.Status = normalizedStatus;
            if (normalizedStatus == AgreementStatuses.Closed)
            {
                agreement.ClosedAtUtc ??= DateTime.UtcNow;
            }
            else
            {
                agreement.ClosedAtUtc = null;
            }
        }
        else
        {
            agreement.Status = ComputeStatusFromApprovals(agreement);
            if (agreement.Status != AgreementStatuses.Closed)
            {
                agreement.ClosedAtUtc = null;
            }
        }

        agreement.LastUpdatedByUserId = currentUser.Id;
        agreement.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToAgreementResponse(agreement));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "SME,Expert,Admin")]
    public async Task<IActionResult> RecordApproval(int id, [FromBody] ApproveAgreementRequest? request)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var agreement = await _dbContext.Agreements.FirstOrDefaultAsync(item => item.Id == id);
        if (agreement == null)
        {
            return NotFound(new { message = "Agreement not found." });
        }

        var targetRole = DetermineApprovalTargetRole(currentUser, request?.TargetRole);
        if (targetRole == null)
        {
            return BadRequest(new { message = "Invalid approval target role. Use SME or Expert." });
        }

        if (targetRole == "SME")
        {
            if (!string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                && agreement.SmeUserId != currentUser.Id)
            {
                return Forbid();
            }

            agreement.SmeApprovedAtUtc = DateTime.UtcNow;
        }
        else
        {
            if (!string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                && agreement.ExpertUserId != currentUser.Id)
            {
                return Forbid();
            }

            agreement.ExpertApprovedAtUtc = DateTime.UtcNow;
        }

        if (agreement.Status != AgreementStatuses.Closed)
        {
            agreement.Status = ComputeStatusFromApprovals(agreement);
        }

        agreement.LastUpdatedByUserId = currentUser.Id;
        agreement.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToAgreementResponse(agreement));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "SME,Expert,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAgreementStatusRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { message = "Status is required." });
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var agreement = await _dbContext.Agreements.FirstOrDefaultAsync(item => item.Id == id);
        if (agreement == null)
        {
            return NotFound(new { message = "Agreement not found." });
        }

        var isAdmin = string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && agreement.SmeUserId != currentUser.Id && agreement.ExpertUserId != currentUser.Id)
        {
            return Forbid();
        }

        var normalizedStatus = NormalizeStatus(request.Status);
        if (normalizedStatus == null)
        {
            return BadRequest(new { message = "Status must be PendingApproval, Active, or Closed." });
        }

        if (normalizedStatus == AgreementStatuses.Active
            && (agreement.SmeApprovedAtUtc == null || agreement.ExpertApprovedAtUtc == null))
        {
            return StatusCode(StatusCodes.Status412PreconditionFailed, new { message = "Both SME and Expert approvals are required before status can be set to Active." });
        }

        agreement.Status = normalizedStatus;
        agreement.ClosedAtUtc = normalizedStatus == AgreementStatuses.Closed ? DateTime.UtcNow : null;
        agreement.LastUpdatedByUserId = currentUser.Id;
        agreement.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToAgreementResponse(agreement));
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .FirstOrDefaultAsync(item => item.Email == normalizedEmail && item.IsActive);
    }

    private static object ToAgreementResponse(AgreementRecord agreement)
    {
        return new
        {
            agreement.Id,
            agreement.ProjectId,
            agreement.ProjectTitle,
            agreement.MilestoneId,
            agreement.SmeUserId,
            agreement.ExpertUserId,
            talentUserId = agreement.ExpertUserId,
            agreement.SmeEmail,
            agreement.ExpertEmail,
            agreement.Amount,
            agreement.Currency,
            agreement.Status,
            agreement.SmeApprovedAtUtc,
            agreement.ExpertApprovedAtUtc,
            agreement.ClosedAtUtc,
            agreement.LastUpdatedByUserId,
            agreement.UpdatedAtUtc
        };
    }

    private static string ComputeStatusFromApprovals(AgreementRecord agreement)
    {
        return agreement.SmeApprovedAtUtc != null && agreement.ExpertApprovedAtUtc != null
            ? AgreementStatuses.Active
            : AgreementStatuses.PendingApproval;
    }

    private static string? NormalizeStatus(string status)
    {
        if (status.Equals(AgreementStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase))
        {
            return AgreementStatuses.PendingApproval;
        }

        if (status.Equals(AgreementStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return AgreementStatuses.Active;
        }

        if (status.Equals(AgreementStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            return AgreementStatuses.Closed;
        }

        return null;
    }

    private static string? DetermineApprovalTargetRole(User currentUser, string? requestedTargetRole)
    {
        if (!string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return currentUser.Role.Equals("SME", StringComparison.OrdinalIgnoreCase)
                ? "SME"
                : currentUser.Role.Equals("Expert", StringComparison.OrdinalIgnoreCase)
                    ? "Expert"
                    : null;
        }

        if (string.IsNullOrWhiteSpace(requestedTargetRole))
        {
            return "SME";
        }

        if (requestedTargetRole.Equals("SME", StringComparison.OrdinalIgnoreCase))
        {
            return "SME";
        }

        if (requestedTargetRole.Equals("Expert", StringComparison.OrdinalIgnoreCase)
            || requestedTargetRole.Equals("Talent", StringComparison.OrdinalIgnoreCase))
        {
            return "Expert";
        }

        return null;
    }
}
