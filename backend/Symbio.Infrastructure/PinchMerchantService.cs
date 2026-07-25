using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Microsoft.Extensions.Configuration;
using Pinch.SDK;
using Pinch.SDK.Merchants;

namespace Symbio.Infrastructure;

public class PinchMerchantService : IPinchMerchantService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;
    private readonly PinchApi? _pinchApi;

    public PinchMerchantService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = PinchApiSettings.FromConfiguration(configuration);
        _pinchApi = HasCredentials() ? CreatePinchApi() : null;
    }

    public Task<SubMerchantRegistrationResult> RegisterSubMerchantAsync(
        string expertEmail,
        string businessIdentifier,
        string companyName,
        CancellationToken cancellationToken = default)
    {
        if (_pinchApi == null)
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
        try
        {
            var response = await _pinchApi!.Merchant.CreateManagedMerchant(new ManagedMerchantCreateOptions
            {
                CompanyName = string.IsNullOrWhiteSpace(companyName) ? "Symbio Expert" : companyName,
                LegalEntityName = string.IsNullOrWhiteSpace(companyName) ? "Symbio Expert" : companyName,
                CompanyEmail = expertEmail,
                CompanyRegistrationNumber = businessIdentifier,
                BankAccountRoutingNumber = "000000",
                BankAccountNumber = "000000000",
                BankAccountName = string.IsNullOrWhiteSpace(companyName) ? "Symbio Expert" : companyName,
                Contacts =
                [
                    new ContactSaveOptions
                {
                        FirstName = InferFirstName(expertEmail),
                        LastName = InferLastName(expertEmail),
                        Email = expertEmail
                    }
                ],
                IpAddress = "127.0.0.1",
                UserAgent = "SymbioHub/1.0"
            });

            var managedMerchant = response.Data;
            if (!response.Success || managedMerchant == null || string.IsNullOrWhiteSpace(managedMerchant.Id))
            {
                return await RegisterMockAsync(expertEmail);
            }

            var merchantId = managedMerchant.Id;
            var onboardingUrl = $"https://connect.getpinch.com.au/glassbox/onboarding/{merchantId}";
            var requiresMore = managedMerchant.Compliance?.Status == "Pending";

            return new SubMerchantRegistrationResult
            {
                MerchantId = merchantId,
                OnboardingUrl = onboardingUrl,
                RequiresAdditionalVerification = requiresMore
            };
        }
        catch
        {
            return await RegisterMockAsync(expertEmail);
        }
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

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApplicationId) && !string.IsNullOrWhiteSpace(_settings.SecretKey);
    }

    private PinchApi CreatePinchApi()
    {
        var isLive = _settings.Environment.Equals("Live", StringComparison.OrdinalIgnoreCase);
        return new PinchApi(_settings.ApplicationId, _settings.SecretKey, isLive);
    }
}