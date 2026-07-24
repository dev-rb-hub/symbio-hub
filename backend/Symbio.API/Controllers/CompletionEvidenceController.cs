using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CompletionEvidenceController : ControllerBase
    {
        private readonly ICompletionEvidenceRepository _completionEvidenceRepository;

        public CompletionEvidenceController(ICompletionEvidenceRepository completionEvidenceRepository)
        {
            _completionEvidenceRepository = completionEvidenceRepository;
        }

        public record FileHashEvidenceRequest(string EpicId, string ArtifactPath, string ArtifactHash, string? SourceCommitSha, string? Notes);
        public record RepositoryReferenceEvidenceRequest(string EpicId, string RepositoryReference, string? SourceCommitSha, string? Notes);

        [HttpPost("file-hash")]
        public async Task<IActionResult> RecordFileHash([FromBody] FileHashEvidenceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EpicId) || string.IsNullOrWhiteSpace(request.ArtifactPath) || string.IsNullOrWhiteSpace(request.ArtifactHash))
            {
                return BadRequest(new { message = "EpicId, ArtifactPath, and ArtifactHash are required." });
            }

            var record = CompletionEvidenceRecord.FromFileHash(
                request.EpicId.Trim(),
                request.ArtifactPath.Trim(),
                request.ArtifactHash.Trim(),
                request.SourceCommitSha,
                request.Notes);

            await _completionEvidenceRepository.RecordAsync(record);

            return Ok(record);
        }

        [HttpPost("repository-reference")]
        public async Task<IActionResult> RecordRepositoryReference([FromBody] RepositoryReferenceEvidenceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EpicId) || string.IsNullOrWhiteSpace(request.RepositoryReference))
            {
                return BadRequest(new { message = "EpicId and RepositoryReference are required." });
            }

            var record = CompletionEvidenceRecord.FromRepositoryReference(
                request.EpicId.Trim(),
                request.RepositoryReference.Trim(),
                request.SourceCommitSha,
                request.Notes);

            await _completionEvidenceRepository.RecordAsync(record);

            return Ok(record);
        }

        [HttpGet("epic/{epicId}")]
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
        public async Task<IActionResult> GetMatrix()
        {
            var matrix = await _completionEvidenceRepository.GetMatrixAsync();
            return Ok(new
            {
                matrix.UpdatedAt,
                records = matrix.Records
            });
        }
    }
}