namespace Symbio.API.Models;

public class AgreementRecord
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = "Kickoff";
    public int SmeUserId { get; set; }
    public User? SmeUser { get; set; }
    public int? ExpertUserId { get; set; }
    public User? ExpertUser { get; set; }
    public string SmeEmail { get; set; } = string.Empty;
    public string? ExpertEmail { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string Status { get; set; } = "PendingApproval";
    public DateTime? SmeApprovedAtUtc { get; set; }
    public DateTime? ExpertApprovedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public int? LastUpdatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
