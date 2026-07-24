using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public class PinchMerchantService : IPinchMerchantService
{
    public Task<SubMerchantRegistrationResult> RegisterSubMerchantAsync(
        string expertEmail,
        string businessIdentifier,
        string companyName,
        CancellationToken cancellationToken = default)
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
}