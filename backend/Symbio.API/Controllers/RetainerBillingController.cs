using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Models;
using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Symbio.Core.Services;

namespace Symbio.API.Controllers;

[ApiController]
[Route("api/retainers")]
public class RetainerBillingController : ControllerBase
{
    private readonly SymbioDbContext _dbContext;
    private readonly IRecurringBillingService _recurringBillingService;
    private readonly IUsageMeteringEngine _usageMeteringEngine;

    public RetainerBillingController(
        SymbioDbContext dbContext,
        IRecurringBillingService recurringBillingService,
        IUsageMeteringEngine usageMeteringEngine)
    {
        _dbContext = dbContext;
        _recurringBillingService = recurringBillingService;
        _usageMeteringEngine = usageMeteringEngine;
    }

    public sealed record CreateRetainerRequest(
        string ProjectId,
        string MilestoneId,
        string ExpertEmail,
        decimal BaseMonthlyAmount,
        string? Currency,
        decimal IncludedSupportHours,
        decimal IncludedCloudUnits,
        decimal OverageRatePerHour,
        decimal OverageRatePerCloudUnit,
        DateTime? StartAtUtc);

    public sealed record AddUsageRequest(
        decimal SupportHours,
        decimal CloudUnits,
        DateTime PeriodStartUtc,
        DateTime PeriodEndUtc);

    [HttpPost]
    [Authorize(Roles = "SME")]
    public async Task<IActionResult> CreateRetainer([FromBody] CreateRetainerRequest request)
    {
        var clientEmail = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(clientEmail))
        {
            return Unauthorized();
        }

        if (request == null
            || string.IsNullOrWhiteSpace(request.ProjectId)
            || string.IsNullOrWhiteSpace(request.MilestoneId)
            || string.IsNullOrWhiteSpace(request.ExpertEmail)
            || request.BaseMonthlyAmount <= 0)
        {
            return BadRequest(new { message = "ProjectId, MilestoneId, ExpertEmail and BaseMonthlyAmount are required." });
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();
        var start = request.StartAtUtc ?? DateTime.UtcNow;

        var plan = await _recurringBillingService.CreatePlanAsync(new RecurringPlanCreateRequest
        {
            Name = $"{request.ProjectId}-{request.MilestoneId}-retainer",
            BaseMonthlyAmount = request.BaseMonthlyAmount,
            Currency = currency,
            Interval = "monthly"
        });

        var subscription = await _recurringBillingService.CreateSubscriptionAsync(new RecurringSubscriptionCreateRequest
        {
            ProviderPlanId = plan.ProviderPlanId,
            ClientEmail = clientEmail,
            ProjectId = request.ProjectId,
            MilestoneId = request.MilestoneId,
            BaseMonthlyAmount = request.BaseMonthlyAmount,
            Currency = currency,
            StartAtUtc = start
        });

        var existing = await _dbContext.RetainerContracts
            .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId && item.MilestoneId == request.MilestoneId);

        if (existing == null)
        {
            existing = new RetainerContractRecord
            {
                ProjectId = request.ProjectId.Trim(),
                MilestoneId = request.MilestoneId.Trim(),
                ClientEmail = clientEmail,
                ExpertEmail = request.ExpertEmail.Trim(),
                ProviderPlanId = plan.ProviderPlanId,
                ProviderSubscriptionId = subscription.ProviderSubscriptionId,
                BaseMonthlyAmount = request.BaseMonthlyAmount,
                Currency = currency,
                IncludedSupportHours = request.IncludedSupportHours,
                IncludedCloudUnits = request.IncludedCloudUnits,
                OverageRatePerHour = request.OverageRatePerHour,
                OverageRatePerCloudUnit = request.OverageRatePerCloudUnit,
                Status = subscription.Status,
                NextBillingAtUtc = subscription.NextBillingAtUtc,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _dbContext.RetainerContracts.Add(existing);
        }
        else
        {
            existing.ExpertEmail = request.ExpertEmail.Trim();
            existing.ProviderPlanId = plan.ProviderPlanId;
            existing.ProviderSubscriptionId = subscription.ProviderSubscriptionId;
            existing.BaseMonthlyAmount = request.BaseMonthlyAmount;
            existing.Currency = currency;
            existing.IncludedSupportHours = request.IncludedSupportHours;
            existing.IncludedCloudUnits = request.IncludedCloudUnits;
            existing.OverageRatePerHour = request.OverageRatePerHour;
            existing.OverageRatePerCloudUnit = request.OverageRatePerCloudUnit;
            existing.Status = subscription.Status;
            existing.NextBillingAtUtc = subscription.NextBillingAtUtc;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            existing.Id,
            existing.ProjectId,
            existing.MilestoneId,
            existing.ProviderPlanId,
            existing.ProviderSubscriptionId,
            existing.Status,
            existing.NextBillingAtUtc
        });
    }

    [HttpPost("{retainerId:int}/usage")]
    [Authorize(Roles = "SME")]
    public async Task<IActionResult> AddUsage(int retainerId, [FromBody] AddUsageRequest request)
    {
        if (request == null || request.PeriodEndUtc <= request.PeriodStartUtc)
        {
            return BadRequest(new { message = "A valid usage period is required." });
        }

        var retainer = await _dbContext.RetainerContracts.FirstOrDefaultAsync(item => item.Id == retainerId);
        if (retainer == null)
        {
            return NotFound(new { message = "Retainer contract not found." });
        }

        var usage = new RetainerUsageRecord
        {
            RetainerContractId = retainer.Id,
            SupportHours = request.SupportHours,
            CloudUnits = request.CloudUnits,
            PeriodStartUtc = request.PeriodStartUtc,
            PeriodEndUtc = request.PeriodEndUtc,
            ProcessedForBilling = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.RetainerUsages.Add(usage);
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            usage.Id,
            usage.RetainerContractId,
            usage.SupportHours,
            usage.CloudUnits,
            usage.PeriodStartUtc,
            usage.PeriodEndUtc,
            usage.ProcessedForBilling
        });
    }

    [HttpGet("control-center")]
    [Authorize(Roles = "SME")]
    public async Task<IActionResult> GetControlCenter()
    {
        var clientEmail = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(clientEmail))
        {
            return Unauthorized();
        }

        var retainers = await _dbContext.RetainerContracts
            .Where(item => item.ClientEmail == clientEmail)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ToListAsync();

        var retainerIds = retainers.Select(item => item.Id).ToList();
        var usages = await _dbContext.RetainerUsages
            .Where(item => retainerIds.Contains(item.RetainerContractId) && !item.ProcessedForBilling)
            .ToListAsync();

        var charges = await _dbContext.RetainerCharges
            .Where(item => retainerIds.Contains(item.RetainerContractId))
            .OrderByDescending(item => item.ChargedAtUtc)
            .Take(100)
            .ToListAsync();

        var retainerViews = retainers.Select(retainer =>
        {
            var pendingUsage = usages.Where(item => item.RetainerContractId == retainer.Id).ToList();
            var usageInput = new MeteredUsageInput
            {
                SupportHours = pendingUsage.Sum(item => item.SupportHours),
                CloudUnits = pendingUsage.Sum(item => item.CloudUnits),
                IncludedSupportHours = retainer.IncludedSupportHours,
                IncludedCloudUnits = retainer.IncludedCloudUnits,
                OverageRatePerHour = retainer.OverageRatePerHour,
                OverageRatePerCloudUnit = retainer.OverageRatePerCloudUnit
            };

            var meteredPreview = _usageMeteringEngine.Calculate(usageInput);

            return new
            {
                retainer.Id,
                retainer.ProjectId,
                retainer.MilestoneId,
                retainer.ExpertEmail,
                retainer.Status,
                retainer.BaseMonthlyAmount,
                retainer.Currency,
                retainer.IncludedSupportHours,
                retainer.IncludedCloudUnits,
                retainer.OverageRatePerHour,
                retainer.OverageRatePerCloudUnit,
                retainer.NextBillingAtUtc,
                pendingUsageHours = usageInput.SupportHours,
                pendingCloudUnits = usageInput.CloudUnits,
                meteredPreview
            };
        });

        return Ok(new
        {
            clientEmail,
            retainers = retainerViews,
            recentCharges = charges
        });
    }
}
