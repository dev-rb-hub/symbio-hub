namespace Symbio.API.Models;

public class AccountingInvoiceRecord
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Provider { get; set; } = "Pinch";
    public string ProviderInvoiceId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Issued";
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string LedgerPayloadJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
