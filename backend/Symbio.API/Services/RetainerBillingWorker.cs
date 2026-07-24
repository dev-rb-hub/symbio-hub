using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Hubs;
using Symbio.API.Models;
using Symbio.Core.Models;
using Symbio.Core.Services;

namespace Symbio.API.Services;

public sealed class RetainerBillingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RetainerBillingWorker> _logger;

    public RetainerBillingWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RetainerBillingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = 30;
        if (int.TryParse(_configuration["Payments:RetainerWorkerIntervalSeconds"], out var parsed) && parsed > 0)
        {
            intervalSeconds = parsed;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRetainers(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retainer billing worker cycle failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessDueRetainers(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
        var meteringEngine = scope.ServiceProvider.GetRequiredService<IUsageMeteringEngine>();
        var accountingHub = scope.ServiceProvider.GetRequiredService<IHubContext<AccountingHub>>();

        var now = DateTime.UtcNow;
        var dueRetainers = await dbContext.RetainerContracts
            .Where(item => item.Status == "Active" && item.NextBillingAtUtc <= now)
            .OrderBy(item => item.NextBillingAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (dueRetainers.Count == 0)
        {
            return;
        }

        foreach (var retainer in dueRetainers)
        {
            var pendingUsage = await dbContext.RetainerUsages
                .Where(item => item.RetainerContractId == retainer.Id && !item.ProcessedForBilling)
                .ToListAsync(cancellationToken);

            var usageInput = new MeteredUsageInput
            {
                SupportHours = pendingUsage.Sum(item => item.SupportHours),
                CloudUnits = pendingUsage.Sum(item => item.CloudUnits),
                IncludedSupportHours = retainer.IncludedSupportHours,
                IncludedCloudUnits = retainer.IncludedCloudUnits,
                OverageRatePerHour = retainer.OverageRatePerHour,
                OverageRatePerCloudUnit = retainer.OverageRatePerCloudUnit
            };

            var metered = meteringEngine.Calculate(usageInput);
            var total = Math.Round(retainer.BaseMonthlyAmount + metered.TotalMeteredAmount, 2, MidpointRounding.AwayFromZero);

            var charge = new RetainerChargeRecord
            {
                RetainerContractId = retainer.Id,
                ProviderSubscriptionId = retainer.ProviderSubscriptionId,
                BaseAmount = retainer.BaseMonthlyAmount,
                MeteredAmount = metered.TotalMeteredAmount,
                TotalAmount = total,
                Currency = retainer.Currency,
                Status = "Paid",
                ProviderReference = $"ret_{retainer.ProviderSubscriptionId}_{now:yyyyMMddHHmmss}",
                ChargedAtUtc = now
            };

            dbContext.RetainerCharges.Add(charge);

            foreach (var usage in pendingUsage)
            {
                usage.ProcessedForBilling = true;
            }

            retainer.NextBillingAtUtc = retainer.NextBillingAtUtc.AddMonths(1);
            retainer.UpdatedAtUtc = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            await accountingHub.Clients.Group(AccountingHub.GetGroupName(retainer.ClientEmail)).SendAsync("RetainerChargePosted", new
            {
                retainerId = retainer.Id,
                retainer.ProjectId,
                retainer.MilestoneId,
                totalAmount = charge.TotalAmount,
                charge.Currency,
                charge.Status,
                charge.ChargedAtUtc,
                retainer.NextBillingAtUtc,
                meteredAmount = charge.MeteredAmount
            }, cancellationToken);
        }
    }
}
