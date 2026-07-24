namespace Symbio.Core.Repositories;

public interface IIdentityVerificationService
{
    Task<bool> ValidateBusinessIdentifierAsync(string businessIdentifier, CancellationToken cancellationToken = default);
}