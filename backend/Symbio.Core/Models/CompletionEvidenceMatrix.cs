namespace Symbio.Core.Models;

public class CompletionEvidenceMatrix
{
    private readonly List<CompletionEvidenceRecord> _records = new();

    public IReadOnlyList<CompletionEvidenceRecord> Records => _records;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void Record(CompletionEvidenceRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (string.IsNullOrWhiteSpace(record.MilestoneId))
        {
            throw new ArgumentException("MilestoneId is required when recording completion evidence.", nameof(record));
        }

        if (string.IsNullOrWhiteSpace(record.EvidenceReferenceValue))
        {
            throw new ArgumentException("EvidenceReferenceValue is required.", nameof(record));
        }

        if (string.IsNullOrWhiteSpace(record.LoggedByEmail))
        {
            throw new ArgumentException("LoggedByEmail is required.", nameof(record));
        }

        // Append-only behavior keeps a full audit trail of all evidence submissions.
        _records.Add(record);
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyList<CompletionEvidenceRecord> ForMilestone(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId))
        {
            return Array.Empty<CompletionEvidenceRecord>();
        }

        return _records
            .Where(item => item.MilestoneId.Equals(milestoneId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.LoggedAtUtc)
            .ToList();
    }

    public IReadOnlyList<CompletionEvidenceRecord> ForEpic(string epicId)
    {
        if (string.IsNullOrWhiteSpace(epicId))
        {
            return Array.Empty<CompletionEvidenceRecord>();
        }

        return _records
            .Where(item => item.EpicId.Equals(epicId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.LoggedAtUtc)
            .ToList();
    }
}