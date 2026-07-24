using Symbio.Core.Models;

namespace Symbio.Core.Repositories;

public interface IPinchMerchantService
{
    Task<SubMerchantRegistrationResult> RegisterSubMerchantAsync(
        string expertEmail,
        string businessIdentifier,
        string companyName,
        CancellationToken cancellationToken = default);
}