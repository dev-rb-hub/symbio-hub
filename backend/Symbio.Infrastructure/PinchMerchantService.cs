using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Symbio.Infrastructure;

public class PinchMerchantService : IPinchMerchantService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;

    public PinchMerchantService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = PinchApiSettings.FromConfiguration(configuration);
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
                companyName = string.IsNullOrWhiteSpace(companyName) ? "Symbio Expert" : companyName,
                companyEmail = expertEmail,
                companyRegistrationNumber = businessIdentifier,
                bankAccountRoutingNumber = "000000",
                bankAccountNumber = "000000000",
                contacts = new[]
                {
                    new
                    {
                        firstName = InferFirstName(expertEmail),
                        lastName = InferLastName(expertEmail),
                        email = expertEmail
                    }
                },
                ipAddress = "127.0.0.1",
                userAgent = "SymbioHub/1.0"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return await RegisterMockAsync(expertEmail);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var merchantId = ReadString(json.RootElement, "merchantId")
            ?? ReadString(json.RootElement, "id")
            ?? ReadString(json.RootElement, "Id")
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

        return ReadString(json.RootElement, "token")
            ?? ReadString(json.RootElement, "access_token")
            ?? ReadString(json.RootElement, "accessToken");
    }

    private static string InferFirstName(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Primary";
        }

        var local = email.Split('@')[0];
        var first = local.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? "Primary" : Capitalize(first);
    }

    private static string InferLastName(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Contact";
        }

        var local = email.Split('@')[0];
        var parts = local.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            return Capitalize(parts[1]);
        }

        return "Contact";
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
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