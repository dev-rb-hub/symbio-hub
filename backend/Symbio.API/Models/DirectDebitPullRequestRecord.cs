namespace Symbio.API.Models;

public class DirectDebitPullRequestRecord
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string PreApprovalProviderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string Status { get; set; } = "Pending";
    public string? ProviderDebitId { get; set; }
    public string? LastError { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
}
