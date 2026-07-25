using Microsoft.Extensions.Configuration;
using Pinch.SDK;
using Pinch.SDK.Payers;
using Pinch.SDK.Plans;
using Pinch.SDK.Subscriptions;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public sealed class PinchRecurringBillingService : IRecurringBillingService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;
    private readonly PinchApi? _pinchApi;

    public PinchRecurringBillingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = PinchApiSettings.FromConfiguration(configuration);
        _pinchApi = HasCredentials() ? CreatePinchApi() : null;
    }

    public async Task<RecurringPlanCreateResult> CreatePlanAsync(RecurringPlanCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockPlan(request);
        }

        if (_pinchApi == null)
        {
            return MockPlan(request);
        }

        try
        {
            var planResponse = await _pinchApi.Plan.Save(new PlanSaveOptions
            {
                Name = request.Name,
                Metadata = $"currency={request.Currency}",
                RecurringPayment = new PlanRecurringPaymentSaveOptions
                {
                    AmountInCents = ToCents(request.BaseMonthlyAmount),
                    Description = $"{request.Name} recurring payment",
                    StartDateOffset = 0,
                    StartDateInterval = "day",
                    FrequencyOffset = 1,
                    FrequencyInterval = request.Interval.Equals("monthly", StringComparison.OrdinalIgnoreCase) ? "month" : request.Interval,
                    EndType = "never"
                }
            });

            var plan = planResponse.Data;
            if (!planResponse.Success || plan == null || string.IsNullOrWhiteSpace(plan.Id))
            {
                return MockPlan(request);
            }

            return new RecurringPlanCreateResult
            {
                ProviderPlanId = plan.Id,
                Status = "Active"
            };
        }
        catch
        {
            return MockPlan(request);
        }
    }

    public async Task<RecurringSubscriptionCreateResult> CreateSubscriptionAsync(RecurringSubscriptionCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockSubscription(request);
        }

        if (_pinchApi == null)
        {
            return MockSubscription(request);
        }

        try
        {
            var payerResponse = await _pinchApi.Payer.Save(new PayerSaveOptions
            {
                EmailAddress = request.ClientEmail,
                Metadata = $"project={request.ProjectId};milestone={request.MilestoneId}"
            });

            var payerId = payerResponse.Data?.Id;
            if (!payerResponse.Success || string.IsNullOrWhiteSpace(payerId))
            {
                return MockSubscription(request);
            }

            var subscriptionResponse = await _pinchApi.Subscriptions.Create(new SubscriptionCreateOptions
            {
                PlanId = request.ProviderPlanId,
                PayerId = payerId,
                StartDate = request.StartAtUtc,
                TotalAmount = ToCents(request.BaseMonthlyAmount)
            });

            var subscription = subscriptionResponse.Data;
            if (!subscriptionResponse.Success || subscription == null || string.IsNullOrWhiteSpace(subscription.Id))
            {
                return MockSubscription(request);
            }

            return new RecurringSubscriptionCreateResult
            {
                ProviderSubscriptionId = subscription.Id,
                Status = string.IsNullOrWhiteSpace(subscription.Status) ? "Active" : subscription.Status,
                NextBillingAtUtc = request.StartAtUtc.AddMonths(1)
            };
        }
        catch
        {
            return MockSubscription(request);
        }
    }

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApplicationId) && !string.IsNullOrWhiteSpace(_settings.SecretKey);
    }

    private PinchApi CreatePinchApi()
    {
        var isLive = _settings.Environment.Equals("Live", StringComparison.OrdinalIgnoreCase);
        return new PinchApi(_settings.ApplicationId, _settings.SecretKey, isLive);
    }

    private static long ToCents(decimal amount)
    {
        return (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }

    private static RecurringPlanCreateResult MockPlan(RecurringPlanCreateRequest request)
    {
        return new RecurringPlanCreateResult
        {
            ProviderPlanId = $"plan_{request.Name.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)}_{DateTime.UtcNow.Ticks}",
            Status = "Active"
        };
    }

    private static RecurringSubscriptionCreateResult MockSubscription(RecurringSubscriptionCreateRequest request)
    {
        return new RecurringSubscriptionCreateResult
        {
            ProviderSubscriptionId = $"sub_{request.ProjectId}_{request.MilestoneId}_{DateTime.UtcNow.Ticks}".Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase),
            Status = "Active",
            NextBillingAtUtc = request.StartAtUtc.AddMonths(1)
        };
    }
}
