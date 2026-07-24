using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public class MockPinchOnboardingGateway : IPinchOnboardingGateway
{
    public Task<PinchOnboardingSession> CreateExpertOnboardingSessionAsync(string expertEmail, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = string.IsNullOrWhiteSpace(expertEmail)
            ? "expert"
            : expertEmail.Trim().ToLowerInvariant();

        var accountId = $"pinch-glassbox-{normalizedEmail.Replace("@", "-").Replace(".", "-")}";

        return Task.FromResult(new PinchOnboardingSession
        {
            ProviderAccountId = accountId,
            OnboardingUrl = $"https://connect.getpinch.com.au/glassbox/onboarding/{accountId}",
            Status = EscrowOnboardingStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    public Task<EscrowOnboardingStatus> GetOnboardingStatusAsync(string providerAccountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerAccountId))
        {
            return Task.FromResult(EscrowOnboardingStatus.NotStarted);
        }

        var normalized = providerAccountId.Trim().ToLowerInvariant();
        if (normalized.Contains("verified"))
        {
            return Task.FromResult(EscrowOnboardingStatus.Verified);
        }

        if (normalized.Contains("rejected"))
        {
            return Task.FromResult(EscrowOnboardingStatus.Rejected);
        }

        return Task.FromResult(EscrowOnboardingStatus.Pending);
    }
}