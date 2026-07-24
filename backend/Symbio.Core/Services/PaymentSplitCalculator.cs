using Symbio.Core.Models;

namespace Symbio.Core.Services;

public class PaymentSplitCalculator : IPaymentSplitCalculator
{
    private const decimal PlatformFeePercent = 10m;
    private const decimal ContractorPercent = 90m;

    public PaymentSplitBreakdown Calculate(decimal grossAmount)
    {
        if (grossAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(grossAmount), "Gross amount must be greater than zero.");
        }

        var platformFee = Math.Round(grossAmount * (PlatformFeePercent / 100m), 2, MidpointRounding.AwayFromZero);
        var contractorAmount = Math.Round(grossAmount - platformFee, 2, MidpointRounding.AwayFromZero);

        return new PaymentSplitBreakdown
        {
            GrossAmount = grossAmount,
            PlatformFeeAmount = platformFee,
            ContractorAmount = contractorAmount,
            PlatformFeePercent = PlatformFeePercent,
            ContractorPercent = ContractorPercent
        };
    }
}