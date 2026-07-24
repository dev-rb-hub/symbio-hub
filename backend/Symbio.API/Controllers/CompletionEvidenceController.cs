using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompletionEvidenceController : ControllerBase
    {
        private readonly ICompletionEvidenceRepository _completionEvidenceRepository;
        private readonly SymbioDbContext _dbContext;

        public CompletionEvidenceController(ICompletionEvidenceRepository completionEvidenceRepository, SymbioDbContext dbContext)
        {
            _completionEvidenceRepository = completionEvidenceRepository;
            _dbContext = dbContext;
        }

        public record FileHashEvidenceRequest(string MilestoneId, string? EpicId, string EvidenceReferenceValue, string? SourceCommitSha, string? Notes, string? TargetDeploymentUrl);
        public record GitCommitEvidenceRequest(string MilestoneId, string? EpicId, string EvidenceReferenceValue, string? SourceCommitSha, string? Notes, string? TargetDeploymentUrl);

        [HttpPost("file-hash")]
        [Authorize(Roles = "Expert,Admin")]
        public async Task<IActionResult> RecordFileHash([FromBody] FileHashEvidenceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MilestoneId) || string.IsNullOrWhiteSpace(request.EvidenceReferenceValue))
            {
                return BadRequest(new { message = "MilestoneId and EvidenceReferenceValue are required." });
            }

            var actor = User.Identity?.Name ?? "unknown";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                var escrowVerified = await IsEscrowVerifiedAsync(actor);
                if (!escrowVerified)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, new
                    {
                        message = "Escrow onboarding must be verified before recording completion evidence."
                    });
                }
            }

            var record = CompletionEvidenceRecord.FromArtifactHash(
                request.MilestoneId.Trim(),
                request.EpicId?.Trim() ?? string.Empty,
                request.EvidenceReferenceValue.Trim(),
                actor,
                request.TargetDeploymentUrl?.Trim() ?? string.Empty,
                request.SourceCommitSha,
                request.Notes);

            await _completionEvidenceRepository.RecordAsync(record);

            return Ok(record);
        }

        [HttpPost("git-commit")]
        [Authorize(Roles = "Expert,Admin")]
        public async Task<IActionResult> RecordGitCommit([FromBody] GitCommitEvidenceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MilestoneId) || string.IsNullOrWhiteSpace(request.EvidenceReferenceValue))
            {
                return BadRequest(new { message = "MilestoneId and EvidenceReferenceValue are required." });
            }

            var actor = User.Identity?.Name ?? "unknown";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                var escrowVerified = await IsEscrowVerifiedAsync(actor);
                if (!escrowVerified)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, new
                    {
                        message = "Escrow onboarding must be verified before recording completion evidence."
                    });
                }
            }

            var record = CompletionEvidenceRecord.FromGitCommit(
                request.MilestoneId.Trim(),
                request.EpicId?.Trim() ?? string.Empty,
                request.EvidenceReferenceValue.Trim(),
                actor,
                request.TargetDeploymentUrl?.Trim() ?? string.Empty,
                request.SourceCommitSha,
                request.Notes);

            await _completionEvidenceRepository.RecordAsync(record);

            return Ok(record);
        }

        [HttpGet("milestone/{milestoneId}")]
        [Authorize(Roles = "Expert,Admin,SME")]
        public async Task<IActionResult> GetByMilestone(string milestoneId)
        {
            if (string.IsNullOrWhiteSpace(milestoneId))
            {
                return BadRequest(new { message = "MilestoneId is required." });
            }

            var records = await _completionEvidenceRepository.GetByMilestoneAsync(milestoneId.Trim());
            return Ok(records);
        }

        [HttpGet("epic/{epicId}")]
        [Authorize(Roles = "Admin,SME")]
        public async Task<IActionResult> GetByEpic(string epicId)
        {
            if (string.IsNullOrWhiteSpace(epicId))
            {
                return BadRequest(new { message = "EpicId is required." });
            }

            var records = await _completionEvidenceRepository.GetByEpicAsync(epicId.Trim());
            return Ok(records);
        }

        [HttpGet("matrix")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMatrix()
        {
            var matrix = await _completionEvidenceRepository.GetMatrixAsync();
            return Ok(new
            {
                matrix.UpdatedAt,
                records = matrix.Records
            });
        }

        [HttpGet("milestone/{milestoneId}/can-settle")]
        [Authorize(Roles = "Admin,SME")]
        public async Task<IActionResult> CanSettleMilestone(string milestoneId)
        {
            if (string.IsNullOrWhiteSpace(milestoneId))
            {
                return BadRequest(new { message = "MilestoneId is required." });
            }

            var records = await _completionEvidenceRepository.GetByMilestoneAsync(milestoneId.Trim());
            var hasVerifiedEvidence = records.Any(item => item.IsVerified);

            var escrowEligibleActor = records
                .OrderByDescending(item => item.LoggedAtUtc)
                .Select(item => item.LoggedByEmail)
                .FirstOrDefault();

            var escrowVerified = !string.IsNullOrWhiteSpace(escrowEligibleActor)
                && await IsEscrowVerifiedAsync(escrowEligibleActor);

            var canSettle = hasVerifiedEvidence && escrowVerified;

            return Ok(new
            {
                milestoneId,
                canSettle,
                reason = canSettle
                    ? "Verified technical delivery evidence is available and expert escrow onboarding is verified."
                    : !hasVerifiedEvidence
                        ? "No verified technical delivery evidence found for this milestone."
                        : "Expert escrow onboarding is not verified for settlement.",
                evidenceCount = records.Count,
                escrowVerified
            });
        }

        private async Task<bool> IsEscrowVerifiedAsync(string expertEmail)
        {
            if (string.IsNullOrWhiteSpace(expertEmail))
            {
                return false;
            }

            var profile = await _dbContext.EscrowOnboardingProfiles
                .FirstOrDefaultAsync(item => item.ExpertEmail == expertEmail);

            return profile != null
                && profile.Status.Equals(EscrowOnboardingStatus.Verified.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}