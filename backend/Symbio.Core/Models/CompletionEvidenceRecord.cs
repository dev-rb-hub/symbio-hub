namespace Symbio.Core.Models;

public class CompletionEvidenceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EpicId { get; set; } = string.Empty;
    public CompletionEvidenceType EvidenceType { get; set; } = CompletionEvidenceType.FileHash;
    public string ArtifactPath { get; set; } = string.Empty;
    public string ArtifactHash { get; set; } = string.Empty;
    public string RepositoryReference { get; set; } = string.Empty;
    public string SourceCommitSha { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    public static CompletionEvidenceRecord FromFileHash(
        string epicId,
        string artifactPath,
        string artifactHash,
        string? sourceCommitSha = null,
        string? notes = null)
    {
        return new CompletionEvidenceRecord
        {
            EpicId = epicId,
            EvidenceType = CompletionEvidenceType.FileHash,
            ArtifactPath = artifactPath,
            ArtifactHash = artifactHash,
            SourceCommitSha = sourceCommitSha ?? string.Empty,
            Notes = notes ?? string.Empty,
            CapturedAt = DateTime.UtcNow
        };
    }

    public static CompletionEvidenceRecord FromRepositoryReference(
        string epicId,
        string repositoryReference,
        string? sourceCommitSha = null,
        string? notes = null)
    {
        return new CompletionEvidenceRecord
        {
            EpicId = epicId,
            EvidenceType = CompletionEvidenceType.RepositoryReference,
            RepositoryReference = repositoryReference,
            SourceCommitSha = sourceCommitSha ?? string.Empty,
            Notes = notes ?? string.Empty,
            CapturedAt = DateTime.UtcNow
        };
    }
}