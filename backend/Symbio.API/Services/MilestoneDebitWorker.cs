using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Symbio.Core.Services;

namespace Symbio.API.Services;

public sealed class MilestoneDebitWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MilestoneDebitWorker> _logger;

    public MilestoneDebitWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<MilestoneDebitWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = 20;
        if (int.TryParse(_configuration["Payments:DebitWorkerIntervalSeconds"], out var parsed) && parsed > 0)
        {
            intervalSeconds = parsed;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingPulls(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Milestone debit worker loop failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessPendingPulls(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
        var pinchDebitService = scope.ServiceProvider.GetRequiredService<IPinchDebitService>();
        var splitCalculator = scope.ServiceProvider.GetRequiredService<IPaymentSplitCalculator>();

        var pending = await dbContext.DirectDebitPullRequests
            .Where(item => item.Status == "Pending")
            .OrderBy(item => item.RequestedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var request in pending)
        {
            request.Status = "Processing";
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await pinchDebitService.ExecuteDirectDebitAsync(new PinchDirectDebitRequest
            {
                ProjectId = request.ProjectId,
                MilestoneId = request.MilestoneId,
                PreApprovalId = request.PreApprovalProviderId,
                Amount = request.Amount,
                Currency = request.Currency
            }, cancellationToken);

            var paymentState = await dbContext.ProjectPaymentStateRecords
                .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId, cancellationToken);

            if (result.Succeeded)
            {
                request.Status = "Succeeded";
                request.ProviderDebitId = result.DebitId;
                request.LastError = null;
                request.ProcessedAtUtc = DateTime.UtcNow;

                var split = splitCalculator.Calculate(request.Amount);
                if (paymentState != null)
                {
                    paymentState.State = "EscrowLocked";
                    paymentState.GrossAmount = split.GrossAmount;
                    paymentState.PlatformFeeAmount = split.PlatformFeeAmount;
                    paymentState.ContractorAmount = split.ContractorAmount;
                    paymentState.Currency = request.Currency;
                    paymentState.LastProviderReference = result.DebitId;
                    paymentState.UpdatedAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                request.Status = "Failed";
                request.LastError = string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Direct debit execution failed." : result.ErrorMessage;
                request.ProcessedAtUtc = DateTime.UtcNow;

                if (paymentState != null)
                {
                    paymentState.State = "DebitFailed";
                    paymentState.LastProviderReference = request.PreApprovalProviderId;
                    paymentState.UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
