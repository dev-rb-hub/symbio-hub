namespace Symbio.API.Models;

public class RetainerUsageRecord
{
    public int Id { get; set; }
    public int RetainerContractId { get; set; }
    public decimal SupportHours { get; set; }
    public decimal CloudUnits { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public bool ProcessedForBilling { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
