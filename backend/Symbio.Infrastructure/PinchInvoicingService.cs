using System.Text.Json;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public sealed class PinchInvoicingService : IAccountingInvoicingService
{
    public Task<AccountingInvoiceResult> TranslateAndCreateInvoiceAsync(AccountingInvoiceCreateRequest request, CancellationToken cancellationToken = default)
    {
        var lines = new List<LedgerLineItem>
        {
            new()
            {
                AccountCode = "400",
                Description = $"Milestone service charge {request.MilestoneId}",
                Amount = request.ContractorAmount,
                TaxType = "GST"
            },
            new()
            {
                AccountCode = "220",
                Description = "Platform processing fee",
                Amount = request.PlatformFeeAmount,
                TaxType = "GST"
            }
        };

        var ledger = new
        {
            schema = "au-standard-ledger-v1",
            providerTargets = new[] { "xero", "myob" },
            projectId = request.ProjectId,
            milestoneId = request.MilestoneId,
            clientEmail = request.ClientEmail,
            settledAtUtc = request.SettledAtUtc,
            currency = request.Currency,
            grossAmount = request.GrossAmount,
            lineItems = lines
        };

        var invoiceId = $"inv_{request.ProjectId}_{request.MilestoneId}_{DateTime.UtcNow.Ticks}";

        return Task.FromResult(new AccountingInvoiceResult
        {
            Provider = "Pinch",
            InvoiceId = invoiceId,
            InvoiceNumber = $"SMB-{DateTime.UtcNow:yyyyMMdd}-{Math.Abs(invoiceId.GetHashCode()) % 100000}",
            Status = "Issued",
            TotalAmount = request.GrossAmount,
            Currency = request.Currency,
            LedgerPayloadJson = JsonSerializer.Serialize(ledger)
        });
    }
}
