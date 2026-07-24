namespace Symbio.API.Models;

public class RetainerChargeRecord
{
    public int Id { get; set; }
    public int RetainerContractId { get; set; }
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public decimal MeteredAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string Status { get; set; } = "Pending";
    public string? ProviderReference { get; set; }
    public DateTime ChargedAtUtc { get; set; } = DateTime.UtcNow;
}
