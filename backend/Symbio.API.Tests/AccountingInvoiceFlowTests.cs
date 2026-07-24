using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Symbio.API.Data;
using Symbio.API.Models;
using Xunit;

namespace Symbio.API.Tests;

public class AccountingInvoiceFlowTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AccountingInvoiceFlowTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Sme_Invoices_Returns_Accounting_Feed()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
            db.AccountingInvoices.Add(new AccountingInvoiceRecord
            {
                ProjectId = "demo-project-epic9-1",
                MilestoneId = "M-1",
                ClientEmail = "sme@example.com",
                Provider = "Pinch",
                ProviderInvoiceId = "inv_test_001",
                InvoiceNumber = "SMB-20260725-00001",
                Status = "Issued",
                TotalAmount = 7500m,
                Currency = "AUD",
                LedgerPayloadJson = "{}",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
            db.ProjectPaymentStateRecords.Add(new ProjectPaymentStateRecord
            {
                ProjectId = "demo-project-epic9-1",
                State = "EscrowLocked",
                GrossAmount = 7500m,
                PlatformFeeAmount = 750m,
                ContractorAmount = 6750m,
                Currency = "AUD",
                UpdatedAtUtc = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var response = await client.GetAsync("/api/payments/sme/invoices");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("count").GetInt32() >= 1);
    }

    [Fact]
    public async Task Accounting_Webhook_Paid_Updates_Invoice_And_Payment_State()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
            db.AccountingInvoices.Add(new AccountingInvoiceRecord
            {
                ProjectId = "demo-project-epic9-2",
                MilestoneId = "M-2",
                ClientEmail = "sme@example.com",
                Provider = "Pinch",
                ProviderInvoiceId = "inv_paid_002",
                InvoiceNumber = "SMB-20260725-00002",
                Status = "Issued",
                TotalAmount = 9200m,
                Currency = "AUD",
                LedgerPayloadJson = "{}",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
            db.ProjectPaymentStateRecords.Add(new ProjectPaymentStateRecord
            {
                ProjectId = "demo-project-epic9-2",
                State = "EscrowLocked",
                GrossAmount = 9200m,
                PlatformFeeAmount = 920m,
                ContractorAmount = 8280m,
                Currency = "AUD",
                UpdatedAtUtc = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/webhooks/accounting-invoices", new
        {
            provider = "Xero",
            providerInvoiceId = "inv_paid_002",
            status = "Paid"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scopeAssert = _factory.Services.CreateScope();
        var dbAssert = scopeAssert.ServiceProvider.GetRequiredService<SymbioDbContext>();
        var invoice = dbAssert.AccountingInvoices.First(item => item.ProviderInvoiceId == "inv_paid_002");
        var state = dbAssert.ProjectPaymentStateRecords.First(item => item.ProjectId == "demo-project-epic9-2");

        Assert.Equal("Paid", invoice.Status);
        Assert.Equal("Paid", state.State);
    }
}
