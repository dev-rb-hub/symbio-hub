namespace Symbio.API.Models;

public class RetainerContractRecord
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ExpertEmail { get; set; } = string.Empty;
    public string ProviderPlanId { get; set; } = string.Empty;
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public decimal BaseMonthlyAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public decimal IncludedSupportHours { get; set; }
    public decimal IncludedCloudUnits { get; set; }
    public decimal OverageRatePerHour { get; set; }
    public decimal OverageRatePerCloudUnit { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime NextBillingAtUtc { get; set; } = DateTime.UtcNow.AddMonths(1);
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
