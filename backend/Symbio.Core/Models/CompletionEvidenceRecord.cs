namespace Symbio.Core.Models;

public class CompletionEvidenceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MilestoneId { get; set; } = string.Empty;
    public string EpicId { get; set; } = string.Empty;
    public CompletionEvidenceType EvidenceType { get; set; } = CompletionEvidenceType.BuildArtifactHash;
    public string EvidenceReferenceValue { get; set; } = string.Empty;
    public string TargetDeploymentUrl { get; set; } = string.Empty;
    public string LoggedByEmail { get; set; } = string.Empty;
    public string SourceCommitSha { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; } = true;
    public string PartitionKey => MilestoneId;

    public static CompletionEvidenceRecord FromArtifactHash(
        string milestoneId,
        string epicId,
        string evidenceReferenceValue,
        string loggedByEmail,
        string targetDeploymentUrl,
        string? sourceCommitSha = null,
        string? notes = null)
    {
        return new CompletionEvidenceRecord
        {
            MilestoneId = milestoneId,
            EpicId = epicId,
            EvidenceType = CompletionEvidenceType.BuildArtifactHash,
            EvidenceReferenceValue = evidenceReferenceValue,
            LoggedByEmail = loggedByEmail,
            TargetDeploymentUrl = targetDeploymentUrl,
            SourceCommitSha = sourceCommitSha ?? string.Empty,
            Notes = notes ?? string.Empty,
            LoggedAtUtc = DateTime.UtcNow,
            IsVerified = true
        };
    }

    public static CompletionEvidenceRecord FromGitCommit(
        string milestoneId,
        string epicId,
        string evidenceReferenceValue,
        string loggedByEmail,
        string targetDeploymentUrl,
        string? sourceCommitSha = null,
        string? notes = null)
    {
        return new CompletionEvidenceRecord
        {
            MilestoneId = milestoneId,
            EpicId = epicId,
            EvidenceType = CompletionEvidenceType.GitCommitSha,
            EvidenceReferenceValue = evidenceReferenceValue,
            LoggedByEmail = loggedByEmail,
            TargetDeploymentUrl = targetDeploymentUrl,
            SourceCommitSha = sourceCommitSha ?? string.Empty,
            Notes = notes ?? string.Empty,
            LoggedAtUtc = DateTime.UtcNow,
            IsVerified = true
        };
    }
}