using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Symbio.Infrastructure;

public class PinchMerchantService : IPinchMerchantService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;

    public PinchMerchantService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = new PinchApiSettings
        {
            BaseUrl = configuration["Pinch:BaseUrl"] ?? "https://api.getpinch.com.au",
            ApiKey = configuration["Pinch:ApiKey"] ?? string.Empty,
            ApiSecret = configuration["Pinch:ApiSecret"] ?? string.Empty,
            WebhookSecret = configuration["Pinch:WebhookSecret"] ?? string.Empty,
            ValidateWebhookSignature = bool.TryParse(configuration["Pinch:ValidateWebhookSignature"], out var validate) && validate,
            TokensPath = configuration["Pinch:TokensPath"] ?? "/tokens",
            ManagedMerchantsPath = configuration["Pinch:ManagedMerchantsPath"] ?? "/managed-merchants"
        };
    }

    public Task<SubMerchantRegistrationResult> RegisterSubMerchantAsync(
        string expertEmail,
        string businessIdentifier,
        string companyName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.ApiSecret))
        {
            return RegisterMockAsync(expertEmail);
        }

        return RegisterWithPinchAsync(expertEmail, businessIdentifier, companyName, cancellationToken);
    }

    private Task<SubMerchantRegistrationResult> RegisterMockAsync(string expertEmail)
    {
        var emailToken = string.IsNullOrWhiteSpace(expertEmail)
            ? "expert"
            : expertEmail.Trim().ToLowerInvariant().Replace("@", "-").Replace(".", "-");

        var merchantId = $"pinch-submerchant-{emailToken}";

        return Task.FromResult(new SubMerchantRegistrationResult
        {
            MerchantId = merchantId,
            OnboardingUrl = $"https://connect.getpinch.com.au/glassbox/onboarding/{merchantId}",
            RequiresAdditionalVerification = false
        });
    }

    private async Task<SubMerchantRegistrationResult> RegisterWithPinchAsync(
        string expertEmail,
        string businessIdentifier,
        string companyName,
        CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return await RegisterMockAsync(expertEmail);
        }

        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var managedMerchantUrl = $"{baseUrl}{NormalizePath(_settings.ManagedMerchantsPath)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, managedMerchantUrl)
        {
            Content = JsonContent.Create(new
            {
                externalReference = expertEmail,
                businessIdentifier,
                businessName = companyName,
                email = expertEmail
            })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return await RegisterMockAsync(expertEmail);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var merchantId = ReadString(json.RootElement, "merchantId")
            ?? ReadString(json.RootElement, "id")
            ?? $"pinch-submerchant-{expertEmail.Trim().ToLowerInvariant().Replace("@", "-").Replace(".", "-")}";

        var onboardingUrl = ReadString(json.RootElement, "onboardingUrl")
            ?? $"https://connect.getpinch.com.au/glassbox/onboarding/{merchantId}";

        var requiresMore = ReadBool(json.RootElement, "requiresAdditionalVerification") ?? false;

        return new SubMerchantRegistrationResult
        {
            MerchantId = merchantId,
            OnboardingUrl = onboardingUrl,
            RequiresAdditionalVerification = requiresMore
        };
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var tokensUrl = $"{baseUrl}{NormalizePath(_settings.TokensPath)}";

        var tokenPayload = new
        {
            key = _settings.ApiKey,
            secret = _settings.ApiSecret
        };

        using var response = await _httpClient.PostAsJsonAsync(tokensUrl, tokenPayload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return ReadString(json.RootElement, "token")
            ?? ReadString(json.RootElement, "access_token")
            ?? ReadString(json.RootElement, "accessToken");
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

    private static bool? ReadBool(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
    }
}