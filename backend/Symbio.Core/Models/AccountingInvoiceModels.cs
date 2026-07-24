namespace Symbio.Core.Models;

public sealed class AccountingInvoiceCreateRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal ContractorAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public DateTime SettledAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class AccountingInvoiceResult
{
    public string Provider { get; set; } = "Pinch";
    public string InvoiceId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Issued";
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string LedgerPayloadJson { get; set; } = "{}";
}

public sealed class LedgerLineItem
{
    public string AccountCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TaxType { get; set; } = "GST";
}
