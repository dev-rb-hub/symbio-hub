using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
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

        public WebhooksController(SymbioDbContext dbContext, IPaymentSplitCalculator paymentSplitCalculator)
        {
            _dbContext = dbContext;
            _paymentSplitCalculator = paymentSplitCalculator;
        }

        public record PinchSettlementWebhookRequest(string ProjectId, string SettlementStatus, decimal Amount, string Currency, string? ProviderReference);

        [HttpPost("pinch-settlements")]
        public async Task<IActionResult> HandlePinchSettlements([FromBody] PinchSettlementWebhookRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectId) || request.Amount <= 0 || string.IsNullOrWhiteSpace(request.SettlementStatus))
            {
                return BadRequest(new { message = "ProjectId, SettlementStatus, and Amount are required." });
            }

            if (!request.SettlementStatus.Equals("confirmed", StringComparison.OrdinalIgnoreCase)
                && !request.SettlementStatus.Equals("succeeded", StringComparison.OrdinalIgnoreCase)
                && !request.SettlementStatus.Equals("escrow_locked", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { message = "Settlement webhook ignored for non-locking status." });
            }

            var state = await _dbContext.ProjectPaymentStateRecords
                .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId);

            if (state == null)
            {
                return NotFound(new { message = "Project payment state not found." });
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
                state.UpdatedAtUtc
            });
        }
    }
}