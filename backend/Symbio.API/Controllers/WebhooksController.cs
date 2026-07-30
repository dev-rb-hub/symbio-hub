using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Hubs;
using Symbio.API.Middleware;
using Symbio.API.Models;
using Symbio.Core.Services;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    [AllowAnonymous]
    public class WebhooksController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;
        private readonly IPaymentSplitCalculator _paymentSplitCalculator;
        private readonly IHubContext<AccountingHub> _accountingHubContext;

        public WebhooksController(
            SymbioDbContext dbContext,
            IPaymentSplitCalculator paymentSplitCalculator,
            IHubContext<AccountingHub> accountingHubContext)
        {
            _dbContext = dbContext;
            _paymentSplitCalculator = paymentSplitCalculator;
            _accountingHubContext = accountingHubContext;
        }

        public sealed record AccountingInvoiceStatusWebhookRequest(
            string Provider,
            string ProviderInvoiceId,
            string Status,
            string? ProjectId,
            string? MilestoneId,
            string? InvoiceNumber);

        public sealed record RetainerSubscriptionStatusWebhookRequest(
            string ProviderSubscriptionId,
            string Status,
            DateTime? NextBillingAtUtc);

        [HttpPost("pinch-settlements")]
        [ServiceFilter(typeof(PinchSignatureValidationFilter))]
        public async Task<IActionResult> HandlePinchSettlements([FromBody] PinchWebhookEnvelope webhookEvent)
        {
            var trustState = ResolveTrustState();
            var trustReason = ResolveTrustReason();
            var authenticityOutcome = new { state = trustState, reason = trustReason };

            if (!PinchWebhookMapper.TryMapSettlementRequest(webhookEvent, out var request) || request == null)
            {
                return BadRequest(new { message = "Webhook payload did not include required settlement data.", trustState, trustReason, authenticityOutcome });
            }

            if (!request.SettlementStatus.Equals("confirmed", StringComparison.OrdinalIgnoreCase)
                && !request.SettlementStatus.Equals("succeeded", StringComparison.OrdinalIgnoreCase)
                && !request.SettlementStatus.Equals("escrow_locked", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { message = "Settlement webhook ignored for non-locking status.", trustState, trustReason, authenticityOutcome });
            }

            var state = await _dbContext.ProjectPaymentStateRecords
                .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId);

            if (state == null)
            {
                return NotFound(new { message = "Project payment state not found.", trustState, trustReason, authenticityOutcome });
            }

            var split = _paymentSplitCalculator.Calculate(request.Amount);
            state.State = "EscrowLocked";
            state.GrossAmount = split.GrossAmount;
            state.PlatformFeeAmount = split.PlatformFeeAmount;
            state.ContractorAmount = split.ContractorAmount;
            state.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();
            state.LastProviderReference = request.ProviderReference?.Trim();
            state.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                state.ProjectId,
                state.State,
                split.GrossAmount,
                split.PlatformFeeAmount,
                split.ContractorAmount,
                state.Currency,
                state.LastProviderReference,
                state.UpdatedAtUtc,
                trustState,
                trustReason,
                authenticityOutcome
            });
        }

        [HttpPost("accounting-invoices")]
        [ServiceFilter(typeof(PinchSignatureValidationFilter))]
        public async Task<IActionResult> HandleAccountingInvoiceStatus([FromBody] AccountingInvoiceStatusWebhookRequest request)
        {
            var trustState = ResolveTrustState();
            var trustReason = ResolveTrustReason();
            var authenticityOutcome = new { state = trustState, reason = trustReason };

            if (request == null || string.IsNullOrWhiteSpace(request.ProviderInvoiceId) || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "ProviderInvoiceId and Status are required.", trustState, trustReason, authenticityOutcome });
            }

            var invoice = await _dbContext.AccountingInvoices
                .FirstOrDefaultAsync(item => item.ProviderInvoiceId == request.ProviderInvoiceId)
                ?? await _dbContext.AccountingInvoices.FirstOrDefaultAsync(item =>
                    !string.IsNullOrWhiteSpace(request.ProjectId)
                    && !string.IsNullOrWhiteSpace(request.MilestoneId)
                    && item.ProjectId == request.ProjectId
                    && item.MilestoneId == request.MilestoneId);

            if (invoice == null)
            {
                return NotFound(new { message = "Invoice record not found.", trustState, trustReason, authenticityOutcome });
            }

            invoice.Status = request.Status.Trim();
            if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
            {
                invoice.InvoiceNumber = request.InvoiceNumber.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.Provider))
            {
                invoice.Provider = request.Provider.Trim();
            }
            invoice.UpdatedAtUtc = DateTime.UtcNow;

            var paymentState = await _dbContext.ProjectPaymentStateRecords
                .FirstOrDefaultAsync(item => item.ProjectId == invoice.ProjectId);

            if (paymentState != null && request.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                paymentState.State = "Paid";
                paymentState.UpdatedAtUtc = DateTime.UtcNow;
                paymentState.LastProviderReference = invoice.ProviderInvoiceId;
            }

            await _dbContext.SaveChangesAsync();

            await _accountingHubContext.Clients.Group(AccountingHub.GetGroupName(invoice.ClientEmail)).SendAsync("InvoiceStatusChanged", new
            {
                invoice.ProjectId,
                invoice.MilestoneId,
                invoice.Provider,
                invoice.ProviderInvoiceId,
                invoice.InvoiceNumber,
                invoiceStatus = invoice.Status,
                paymentState = paymentState?.State ?? "Unknown",
                invoice.TotalAmount,
                invoice.Currency,
                updatedAtUtc = invoice.UpdatedAtUtc
            });

            return Ok(new
            {
                invoice.ProviderInvoiceId,
                invoice.Status,
                paymentState = paymentState?.State ?? "Unknown",
                invoice.UpdatedAtUtc,
                trustState,
                trustReason,
                authenticityOutcome
            });
        }

        [HttpPost("pinch-subscriptions")]
        [ServiceFilter(typeof(PinchSignatureValidationFilter))]
        public async Task<IActionResult> HandleRetainerSubscriptionStatus([FromBody] RetainerSubscriptionStatusWebhookRequest request)
        {
            var trustState = ResolveTrustState();
            var trustReason = ResolveTrustReason();
            var authenticityOutcome = new { state = trustState, reason = trustReason };

            if (request == null || string.IsNullOrWhiteSpace(request.ProviderSubscriptionId) || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "ProviderSubscriptionId and Status are required.", trustState, trustReason, authenticityOutcome });
            }

            var retainer = await _dbContext.RetainerContracts
                .FirstOrDefaultAsync(item => item.ProviderSubscriptionId == request.ProviderSubscriptionId);

            if (retainer == null)
            {
                return NotFound(new { message = "Retainer contract not found.", trustState, trustReason, authenticityOutcome });
            }

            retainer.Status = request.Status.Trim();
            if (request.NextBillingAtUtc.HasValue)
            {
                retainer.NextBillingAtUtc = request.NextBillingAtUtc.Value;
            }
            retainer.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _accountingHubContext.Clients.Group(AccountingHub.GetGroupName(retainer.ClientEmail)).SendAsync("RetainerStatusChanged", new
            {
                retainer.Id,
                retainer.ProjectId,
                retainer.MilestoneId,
                retainer.ProviderSubscriptionId,
                retainer.Status,
                retainer.NextBillingAtUtc,
                retainer.UpdatedAtUtc
            });

            return Ok(new
            {
                retainer.Id,
                retainer.ProviderSubscriptionId,
                retainer.Status,
                retainer.NextBillingAtUtc,
                retainer.UpdatedAtUtc,
                trustState,
                trustReason,
                authenticityOutcome
            });
        }

        private string ResolveTrustState()
        {
            if (HttpContext.Items.TryGetValue(PinchWebhookTrustContext.ItemKey, out var trustState)
                && trustState is string trustStateText
                && !string.IsNullOrWhiteSpace(trustStateText))
            {
                return trustStateText;
            }

            return PinchWebhookTrustContext.BypassedState;
        }

        private string ResolveTrustReason()
        {
            if (HttpContext.Items.TryGetValue(PinchWebhookTrustContext.ReasonItemKey, out var trustReason)
                && trustReason is string trustReasonText
                && !string.IsNullOrWhiteSpace(trustReasonText))
            {
                return trustReasonText;
            }

            return PinchSignatureValidationStatus.SignatureValidationBypassed.ToString();
        }
    }
}