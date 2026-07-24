namespace Symbio.API.Models;

public class PaymentPreApprovalRecord
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string BsbMasked { get; set; } = string.Empty;
    public string AccountNumberMasked { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string ProviderPreApprovalId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAtUtc { get; set; }
}
