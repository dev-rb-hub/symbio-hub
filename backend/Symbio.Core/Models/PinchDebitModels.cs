namespace Symbio.Core.Models;

public sealed class PinchPreApprovalRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? SourceToken { get; set; }
    public string Bsb { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
}

public sealed class PinchPreApprovalResult
{
    public string PreApprovalId { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string Status { get; set; } = "Pending";
}

public sealed class PinchDirectDebitRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string MilestoneId { get; set; } = string.Empty;
    public string PreApprovalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
}

public sealed class PinchDirectDebitResult
{
    public string DebitId { get; set; } = string.Empty;
    public bool Succeeded { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
}
