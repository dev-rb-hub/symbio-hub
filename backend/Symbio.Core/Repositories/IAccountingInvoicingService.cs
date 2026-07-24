using Symbio.Core.Models;

namespace Symbio.Core.Repositories;

public interface IAccountingInvoicingService
{
    Task<AccountingInvoiceResult> TranslateAndCreateInvoiceAsync(AccountingInvoiceCreateRequest request, CancellationToken cancellationToken = default);
}
