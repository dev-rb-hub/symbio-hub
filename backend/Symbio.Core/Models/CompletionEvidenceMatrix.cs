namespace Symbio.Core.Models;

public class CompletionEvidenceMatrix
{
    private readonly List<CompletionEvidenceRecord> _records = new();

    public IReadOnlyList<CompletionEvidenceRecord> Records => _records;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void Record(CompletionEvidenceRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (string.IsNullOrWhiteSpace(record.EpicId))
        {
            throw new ArgumentException("EpicId is required when recording completion evidence.", nameof(record));
        }

        if (record.EvidenceType == CompletionEvidenceType.FileHash)
        {
            if (string.IsNullOrWhiteSpace(record.ArtifactPath) || string.IsNullOrWhiteSpace(record.ArtifactHash))
            {
                throw new ArgumentException("File-hash evidence must include both ArtifactPath and ArtifactHash.", nameof(record));
            }

            var existing = _records.FirstOrDefault(item =>
                item.EpicId.Equals(record.EpicId, StringComparison.OrdinalIgnoreCase)
                && item.EvidenceType == CompletionEvidenceType.FileHash
                && item.ArtifactPath.Equals(record.ArtifactPath, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                existing.ArtifactHash = record.ArtifactHash;
                existing.SourceCommitSha = record.SourceCommitSha;
                existing.Notes = record.Notes;
                existing.CapturedAt = DateTime.UtcNow;
                UpdatedAt = DateTime.UtcNow;
                return;
            }
        }

        if (record.EvidenceType == CompletionEvidenceType.RepositoryReference)
        {
            if (string.IsNullOrWhiteSpace(record.RepositoryReference))
            {
                throw new ArgumentException("Repository-reference evidence must include RepositoryReference.", nameof(record));
            }
        }

        _records.Add(record);
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyList<CompletionEvidenceRecord> ForEpic(string epicId)
    {
        if (string.IsNullOrWhiteSpace(epicId))
        {
            return Array.Empty<CompletionEvidenceRecord>();
        }

        return _records
            .Where(item => item.EpicId.Equals(epicId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.CapturedAt)
            .ToList();
    }
}