namespace Symbio.API.Models;

public class AdminProjectFlagRecord
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string ReportedByEmail { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string? ResolvedByEmail { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
}
