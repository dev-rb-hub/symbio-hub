using Symbio.Core.Models;

namespace Symbio.Core.Services;

public interface IUsageMeteringEngine
{
    MeteredChargeBreakdown Calculate(MeteredUsageInput input);
}
