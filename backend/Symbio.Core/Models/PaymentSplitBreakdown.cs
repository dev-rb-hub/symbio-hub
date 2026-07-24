namespace Symbio.Core.Models;

public class PaymentSplitBreakdown
{
    public decimal GrossAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal ContractorAmount { get; set; }
    public decimal PlatformFeePercent { get; set; }
    public decimal ContractorPercent { get; set; }
}