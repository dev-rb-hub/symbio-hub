using Symbio.Core.Models;

namespace Symbio.Core.Services;

public interface IPaymentSplitCalculator
{
    PaymentSplitBreakdown Calculate(decimal grossAmount);
}