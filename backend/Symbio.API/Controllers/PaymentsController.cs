using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Models;
using Symbio.Core.Repositories;
using Symbio.Core.Services;
using Symbio.Core.Models;

namespace Symbio.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private static readonly Regex BsbRegex = new("^\\d{3}-?\\d{3}$", RegexOptions.Compiled);
    private static readonly Regex AccountNumberRegex = new("^\\d{3,9}$", RegexOptions.Compiled);

    private readonly SymbioDbContext _dbContext;
    private readonly IPinchDebitService _pinchDebitService;
    private readonly IPaymentSplitCalculator _splitCalculator;

    public PaymentsController(
        SymbioDbContext dbContext,
        IPinchDebitService pinchDebitService,
        IPaymentSplitCalculator splitCalculator)
    {
        _dbContext = dbContext;
        _pinchDebitService = pinchDebitService;
        _splitCalculator = splitCalculator;
    }

    public sealed record CapturePreApprovalRequest(
        string ProjectId,
        string MilestoneId,
        string AccountName,
        string? SourceToken,
        string Bsb,
        string AccountNumber,
        decimal Amount,
        string? Currency);

    public sealed record QueueMilestoneDebitRequest(
        string ProjectId,
        string MilestoneId,
        decimal? Amount,
        string? Currency);

    [HttpGet("runtime-mode")]
    [Authorize(Roles = "SME,Expert,Admin")]
    public IActionResult GetRuntimeMode()
    {
        var runtimeMode = _pinchDebitService.GetRuntimeMode();
        return Ok(new
        {
            runtimeMode.ModeLabel,
            runtimeMode.Environment,
            runtimeMode.CredentialsConfigured,
            runtimeMode.UsesMockResponses,
            runtimeMode.BaseUri,
            runtimeMode.AuthUri,
            runtimeMode.IsLive,
            guidance = runtimeMode.UsesMockResponses
                ? "Mock mode active: settlement and pre-approval responses are simulated until Pinch credentials are configured."
                : "Pinch integration credentials are configured for this environment."
        });
    }

    [HttpGet("pinch/sandbox-verification")]
    [Authorize(Roles = "SME,Expert,Admin")]
    public async Task<IActionResult> VerifyPinchSandboxConnection()
    {
        var verification = await _pinchDebitService.VerifySandboxConnectionAsync();

        return Ok(new
        {
            verification.ModeLabel,
            verification.Environment,
            verification.CredentialsConfigured,
            verification.Connected,
            verification.MerchantReadSucceeded,
            verification.PayerListReadSucceeded,
            verification.BaseUri,
            verification.AuthUri,
            verification.IsLive,
            verification.Message,
            verification.MerchantName,
            verification.FailureReason,
            verification.MerchantReadErrorCode,
            verification.MerchantReadErrorMessage,
            verification.PayerListErrorCode,
            verification.PayerListErrorMessage,
            verification.PayerListErrorCount
        });
    }

    [HttpGet("sme/invoices")]
    [Authorize(Roles = "SME")]
    public async Task<IActionResult> GetSmeInvoices()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var invoices = await _dbContext.AccountingInvoices
            .Where(item => item.ClientEmail == email)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .Take(50)
            .ToListAsync();

        var projectIds = invoices.Select(item => item.ProjectId).Distinct().ToList();
        var states = await _dbContext.ProjectPaymentStateRecords
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToDictionaryAsync(item => item.ProjectId, item => item);

        var results = invoices.Select(item => new
        {
            item.ProjectId,
            item.MilestoneId,
            item.Provider,
            item.ProviderInvoiceId,
            item.InvoiceNumber,
            invoiceStatus = item.Status,
            paymentState = states.TryGetValue(item.ProjectId, out var state) ? state.State : "Unknown",
            item.TotalAmount,
            item.Currency,
            item.CreatedAtUtc,
            item.UpdatedAtUtc
        });

        return Ok(new
        {
            clientEmail = email,
            count = invoices.Count,
            invoices = results
        });
    }

    [HttpPost("pre-approvals")]
    [Authorize(Roles = "SME")]
    public async Task<IActionResult> CapturePreApproval([FromBody] CapturePreApprovalRequest request)
    {
        var clientEmail = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(clientEmail))
        {
            return Unauthorized();
        }

        if (request == null
            || string.IsNullOrWhiteSpace(request.ProjectId)
            || string.IsNullOrWhiteSpace(request.MilestoneId)
            || string.IsNullOrWhiteSpace(request.AccountName)
            || request.Amount < 5000m)
        {
            return BadRequest(new { message = "ProjectId, MilestoneId, AccountName, and Amount (>= 5000) are required." });
        }

        var hasSourceToken = !string.IsNullOrWhiteSpace(request.SourceToken);
        var normalizedBsb = NormalizeBsb(request.Bsb);
        var normalizedAccount = request.AccountNumber?.Replace(" ", string.Empty, StringComparison.Ordinal) ?? string.Empty;

        if (!hasSourceToken && (!BsbRegex.IsMatch(normalizedBsb) || !AccountNumberRegex.IsMatch(normalizedAccount)))
        {
            return BadRequest(new { message = "Please provide a valid Australian BSB and account number." });
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();

        var result = await _pinchDebitService.CreatePreApprovalAsync(new PinchPreApprovalRequest
        {
            ProjectId = request.ProjectId.Trim(),
            MilestoneId = request.MilestoneId.Trim(),
            CustomerEmail = clientEmail,
            AccountName = request.AccountName.Trim(),
            SourceToken = request.SourceToken?.Trim(),
            Bsb = normalizedBsb,
            AccountNumber = normalizedAccount,
            Amount = request.Amount,
            Currency = currency
        });

        var record = await _dbContext.PaymentPreApprovals
            .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId && item.MilestoneId == request.MilestoneId);

        if (record == null)
        {
            record = new PaymentPreApprovalRecord
            {
                ProjectId = request.ProjectId.Trim(),
                MilestoneId = request.MilestoneId.Trim(),
                ClientEmail = clientEmail,
                Amount = request.Amount,
                Currency = currency,
                BsbMasked = MaskBsb(normalizedBsb),
                AccountNumberMasked = MaskAccountNumber(normalizedAccount),
                Status = result.Status,
                ProviderPreApprovalId = result.PreApprovalId,
                CreatedAtUtc = DateTime.UtcNow,
                ApprovedAtUtc = result.IsApproved ? DateTime.UtcNow : null
            };
            _dbContext.PaymentPreApprovals.Add(record);
        }
        else
        {
            record.ClientEmail = clientEmail;
            record.Amount = request.Amount;
            record.Currency = currency;
            record.BsbMasked = MaskBsb(normalizedBsb);
            record.AccountNumberMasked = MaskAccountNumber(normalizedAccount);
            record.Status = result.Status;
            record.ProviderPreApprovalId = result.PreApprovalId;
            record.ApprovedAtUtc = result.IsApproved ? DateTime.UtcNow : null;
        }

        var state = await _dbContext.ProjectPaymentStateRecords.FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId);
        if (state == null)
        {
            var split = _splitCalculator.Calculate(request.Amount);
            state = new ProjectPaymentStateRecord
            {
                ProjectId = request.ProjectId.Trim(),
                State = result.IsApproved ? "PreApprovalApproved" : "PreApprovalPending",
                GrossAmount = split.GrossAmount,
                PlatformFeeAmount = split.PlatformFeeAmount,
                ContractorAmount = split.ContractorAmount,
                Currency = currency,
                LastProviderReference = result.PreApprovalId,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _dbContext.ProjectPaymentStateRecords.Add(state);
        }
        else
        {
            state.State = result.IsApproved ? "PreApprovalApproved" : "PreApprovalPending";
            state.GrossAmount = request.Amount;
            state.Currency = currency;
            state.LastProviderReference = result.PreApprovalId;
            state.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            record.ProjectId,
            record.MilestoneId,
            record.Status,
            record.ProviderPreApprovalId,
            record.BsbMasked,
            record.AccountNumberMasked,
            state = state.State
        });
    }

    [HttpPost("milestones/sign-off")]
    [Authorize(Roles = "SME")]
    public async Task<IActionResult> QueueDebitAfterSignOff([FromBody] QueueMilestoneDebitRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ProjectId) || string.IsNullOrWhiteSpace(request.MilestoneId))
        {
            return BadRequest(new { message = "ProjectId and MilestoneId are required." });
        }

        var preApproval = await _dbContext.PaymentPreApprovals
            .Where(item => item.ProjectId == request.ProjectId && item.MilestoneId == request.MilestoneId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (preApproval == null || !preApproval.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status412PreconditionFailed, new { message = "An approved pre-approval is required before sign-off can queue a debit pull." });
        }

        var alreadyQueued = await _dbContext.DirectDebitPullRequests
            .AnyAsync(item => item.ProjectId == request.ProjectId && item.MilestoneId == request.MilestoneId && (item.Status == "Pending" || item.Status == "Processing"));

        if (alreadyQueued)
        {
            return Conflict(new { message = "A debit pull is already queued for this milestone." });
        }

        var amount = request.Amount.GetValueOrDefault(preApproval.Amount);
        if (amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero." });
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? preApproval.Currency : request.Currency.Trim().ToUpperInvariant();

        var queued = new DirectDebitPullRequestRecord
        {
            ProjectId = request.ProjectId.Trim(),
            MilestoneId = request.MilestoneId.Trim(),
            PreApprovalProviderId = preApproval.ProviderPreApprovalId,
            Amount = amount,
            Currency = currency,
            Status = "Pending",
            RequestedAtUtc = DateTime.UtcNow
        };

        _dbContext.DirectDebitPullRequests.Add(queued);

        var state = await _dbContext.ProjectPaymentStateRecords
            .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId);

        if (state == null)
        {
            state = new ProjectPaymentStateRecord
            {
                ProjectId = request.ProjectId.Trim(),
                State = "DebitQueued",
                GrossAmount = amount,
                Currency = currency,
                LastProviderReference = preApproval.ProviderPreApprovalId,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _dbContext.ProjectPaymentStateRecords.Add(state);
        }
        else
        {
            state.State = "DebitQueued";
            state.GrossAmount = amount;
            state.Currency = currency;
            state.LastProviderReference = preApproval.ProviderPreApprovalId;
            state.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return Accepted(new
        {
            queued.ProjectId,
            queued.MilestoneId,
            queued.Amount,
            queued.Currency,
            queued.Status,
            state = state.State
        });
    }

    private static string NormalizeBsb(string bsb)
    {
        var digits = (bsb ?? string.Empty).Replace("-", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal);
        if (digits.Length != 6)
        {
            return bsb ?? string.Empty;
        }

        return $"{digits[..3]}-{digits[3..]}";
    }

    private static string MaskBsb(string bsb)
    {
        var normalized = NormalizeBsb(bsb);
        return normalized.Length == 7 ? $"***-{normalized[4..]}" : "***-***";
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return "***";
        }

        var trimmed = accountNumber.Trim();
        var visible = trimmed.Length > 3 ? trimmed[^3..] : trimmed;
        return new string('*', Math.Max(0, trimmed.Length - visible.Length)) + visible;
    }
}
