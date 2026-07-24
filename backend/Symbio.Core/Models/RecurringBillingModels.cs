namespace Symbio.Core.Models;

public sealed class RecurringPlanCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal BaseMonthlyAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string Interval { get; set; } = "monthly";
}

public sealed class RecurringPlanCreateResult
{
    public string ProviderPlanId { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public sealed class RecurringSubscriptionCreateRequest
{
    public string ProviderPlanId { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public decimal BaseMonthlyAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public DateTime StartAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class RecurringSubscriptionCreateResult
{
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime NextBillingAtUtc { get; set; } = DateTime.UtcNow.AddMonths(1);
}

public sealed class MeteredUsageInput
{
    public decimal SupportHours { get; set; }
    public decimal CloudUnits { get; set; }
    public decimal IncludedSupportHours { get; set; }
    public decimal IncludedCloudUnits { get; set; }
    public decimal OverageRatePerHour { get; set; }
    public decimal OverageRatePerCloudUnit { get; set; }
}

public sealed class MeteredChargeBreakdown
{
    public decimal BillableSupportHours { get; set; }
    public decimal BillableCloudUnits { get; set; }
    public decimal SupportOverageAmount { get; set; }
    public decimal CloudOverageAmount { get; set; }
    public decimal TotalMeteredAmount { get; set; }
}
