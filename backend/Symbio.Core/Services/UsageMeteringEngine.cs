using Symbio.Core.Models;

namespace Symbio.Core.Services;

public sealed class UsageMeteringEngine : IUsageMeteringEngine
{
    public MeteredChargeBreakdown Calculate(MeteredUsageInput input)
    {
        var billableSupportHours = Math.Max(0, input.SupportHours - input.IncludedSupportHours);
        var billableCloudUnits = Math.Max(0, input.CloudUnits - input.IncludedCloudUnits);

        var supportOverageAmount = Math.Round(billableSupportHours * input.OverageRatePerHour, 2, MidpointRounding.AwayFromZero);
        var cloudOverageAmount = Math.Round(billableCloudUnits * input.OverageRatePerCloudUnit, 2, MidpointRounding.AwayFromZero);

        return new MeteredChargeBreakdown
        {
            BillableSupportHours = billableSupportHours,
            BillableCloudUnits = billableCloudUnits,
            SupportOverageAmount = supportOverageAmount,
            CloudOverageAmount = cloudOverageAmount,
            TotalMeteredAmount = supportOverageAmount + cloudOverageAmount
        };
    }
}
