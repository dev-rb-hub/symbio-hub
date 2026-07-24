using Symbio.Core.Models;

namespace Symbio.Core.Repositories;

public interface IRecurringBillingService
{
    Task<RecurringPlanCreateResult> CreatePlanAsync(RecurringPlanCreateRequest request, CancellationToken cancellationToken = default);
    Task<RecurringSubscriptionCreateResult> CreateSubscriptionAsync(RecurringSubscriptionCreateRequest request, CancellationToken cancellationToken = default);
}
