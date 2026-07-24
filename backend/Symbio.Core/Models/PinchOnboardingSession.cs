namespace Symbio.Core.Models;

public class PinchOnboardingSession
{
    public string ProviderAccountId { get; set; } = string.Empty;
    public string OnboardingUrl { get; set; } = string.Empty;
    public EscrowOnboardingStatus Status { get; set; } = EscrowOnboardingStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}