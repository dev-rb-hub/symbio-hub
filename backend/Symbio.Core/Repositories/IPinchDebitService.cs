using Symbio.Core.Models;

namespace Symbio.Core.Repositories;

public interface IPinchDebitService
{
    Task<PinchPreApprovalResult> CreatePreApprovalAsync(PinchPreApprovalRequest request, CancellationToken cancellationToken = default);
    Task<PinchDirectDebitResult> ExecuteDirectDebitAsync(PinchDirectDebitRequest request, CancellationToken cancellationToken = default);
}
