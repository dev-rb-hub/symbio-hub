using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public sealed class PinchRecurringBillingService : IRecurringBillingService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;

    public PinchRecurringBillingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = PinchApiSettings.FromConfiguration(configuration);
    }

    public async Task<RecurringPlanCreateResult> CreatePlanAsync(RecurringPlanCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockPlan(request);
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return MockPlan(request);
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{NormalizePath(_settings.PlansPath)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                name = request.Name,
                amount = request.BaseMonthlyAmount,
                currency = request.Currency,
                interval = request.Interval
            })
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return MockPlan(request);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return new RecurringPlanCreateResult
        {
            ProviderPlanId = ReadString(json.RootElement, "id") ?? ReadString(json.RootElement, "planId") ?? $"plan_{Guid.NewGuid():N}",
            Status = ReadString(json.RootElement, "status") ?? "Active"
        };
    }

    public async Task<RecurringSubscriptionCreateResult> CreateSubscriptionAsync(RecurringSubscriptionCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockSubscription(request);
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return MockSubscription(request);
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{NormalizePath(_settings.SubscriptionsPath)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                planId = request.ProviderPlanId,
                customerEmail = request.ClientEmail,
                metadata = new
                {
                    projectId = request.ProjectId,
                    milestoneId = request.MilestoneId,
                    baseMonthlyAmount = request.BaseMonthlyAmount
                },
                startDate = request.StartAtUtc
            })
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return MockSubscription(request);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return new RecurringSubscriptionCreateResult
        {
            ProviderSubscriptionId = ReadString(json.RootElement, "id") ?? ReadString(json.RootElement, "subscriptionId") ?? $"sub_{Guid.NewGuid():N}",
            Status = ReadString(json.RootElement, "status") ?? "Active",
            NextBillingAtUtc = DateTime.UtcNow.AddMonths(1)
        };
    }

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApiKey) && !string.IsNullOrWhiteSpace(_settings.ApiSecret);
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var authBaseUrl = _settings.AuthBaseUrl.TrimEnd('/');
        var tokensUrl = $"{authBaseUrl}{NormalizePath(_settings.TokensPath)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, tokensUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = "api1"
            })
        };

        var basicToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ApiKey}:{_settings.ApiSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return ReadString(json.RootElement, "access_token") ?? ReadString(json.RootElement, "token");
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

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.StartsWith('/') ? path : $"/{path}";
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
