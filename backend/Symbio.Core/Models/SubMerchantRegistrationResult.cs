namespace Symbio.Core.Models;

public class SubMerchantRegistrationResult
{
    public string MerchantId { get; set; } = string.Empty;
    public string OnboardingUrl { get; set; } = string.Empty;
    public bool RequiresAdditionalVerification { get; set; }
}