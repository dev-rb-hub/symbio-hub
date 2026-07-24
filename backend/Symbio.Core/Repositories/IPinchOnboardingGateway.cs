using Symbio.Core.Models;

namespace Symbio.Core.Repositories;

public interface IPinchOnboardingGateway
{
    Task<PinchOnboardingSession> CreateExpertOnboardingSessionAsync(string expertEmail, CancellationToken cancellationToken = default);
    Task<EscrowOnboardingStatus> GetOnboardingStatusAsync(string providerAccountId, CancellationToken cancellationToken = default);
}