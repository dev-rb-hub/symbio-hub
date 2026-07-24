using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public CompletionEvidenceController(ICompletionEvidenceRepository completionEvidenceRepository)
        {
            _completionEvidenceRepository = completionEvidenceRepository;
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

            return Ok(new
            {
                milestoneId,
                canSettle = hasVerifiedEvidence,
                reason = hasVerifiedEvidence
                    ? "Verified technical delivery evidence is available."
                    : "No verified technical delivery evidence found for this milestone.",
                evidenceCount = records.Count
            });
        }
    }
}