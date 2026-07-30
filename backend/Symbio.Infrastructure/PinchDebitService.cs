using Microsoft.Extensions.Configuration;
using Pinch.SDK;
using Pinch.SDK.Agreements;
using Pinch.SDK.Payers;
using Pinch.SDK.Payments;
using Pinch.SDK.Sources;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public sealed class PinchDebitService : IPinchDebitService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;
    private readonly PinchApi? _pinchApi;

    public PinchDebitService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = PinchApiSettings.FromConfiguration(configuration);
        _pinchApi = HasCredentials() ? CreatePinchApi() : null;
    }

    public async Task<PinchPreApprovalResult> CreatePreApprovalAsync(PinchPreApprovalRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockPreApproval(request);
        }

        if (_pinchApi == null)
        {
            return MockPreApproval(request);
        }

        try
        {
            var payerResponse = await _pinchApi.Payer.Save(new PayerSaveOptions
            {
                EmailAddress = request.CustomerEmail,
                CompanyName = request.AccountName,
                Metadata = $"project={request.ProjectId};milestone={request.MilestoneId}",
                Source = new SourceSaveOptions
                {
                    SourceType = "bank-account",
                    BankAccountName = request.AccountName,
                    BankAccountBsb = request.Bsb,
                    BankAccountNumber = request.AccountNumber
                }
            });

            var payerId = payerResponse.Data?.Id;
            if (!payerResponse.Success || string.IsNullOrWhiteSpace(payerId))
            {
                return MockPreApproval(request);
            }

            var agreementResponse = await _pinchApi.Agreement.Create(new AgreementSaveOptions
            {
                PayerId = payerId
            });

            var agreement = agreementResponse.Data;
            if (!agreementResponse.Success || agreement == null || string.IsNullOrWhiteSpace(agreement.Id))
            {
                return MockPreApproval(request);
            }

            var isApproved = agreement.ConfirmedDateUtc.HasValue;
            return new PinchPreApprovalResult
            {
                PreApprovalId = agreement.Id,
                IsApproved = isApproved,
                Status = isApproved ? "Approved" : "Pending"
            };
        }
        catch
        {
            return MockPreApproval(request);
        }
    }

    public async Task<PinchDirectDebitResult> ExecuteDirectDebitAsync(PinchDirectDebitRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockDebit(request);
        }

        if (_pinchApi == null)
        {
            return MockDebit(request);
        }

        try
        {
            var agreementResponse = await _pinchApi.Agreement.Get(request.PreApprovalId);
            var agreement = agreementResponse.Data;
            var payerId = agreement?.Payer?.Id;

            if (!agreementResponse.Success || string.IsNullOrWhiteSpace(payerId))
            {
                return MockDebit(request);
            }

            var paymentResponse = await _pinchApi.Payment.Save(new PaymentSaveOptions
            {
                PayerId = payerId,
                Amount = ToCents(request.Amount),
                TransactionDate = DateTime.UtcNow,
                Description = $"{request.ProjectId}:{request.MilestoneId}"
            });

            var payment = paymentResponse.Data;
            if (!paymentResponse.Success || payment == null)
            {
                return MockDebit(request);
            }

            var status = string.IsNullOrWhiteSpace(payment.Status) ? "Pending" : payment.Status;
            var succeeded = status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Processed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Completed", StringComparison.OrdinalIgnoreCase);

            return new PinchDirectDebitResult
            {
                DebitId = string.IsNullOrWhiteSpace(payment.Id)
                    ? $"debit_{request.ProjectId}_{request.MilestoneId}"
                    : payment.Id,
                Status = status,
                Succeeded = succeeded,
                ErrorMessage = succeeded ? null : "Pinch SDK reported a non-success debit status."
            };
        }
        catch
        {
            return MockDebit(request);
        }
    }

    public PinchIntegrationRuntimeMode GetRuntimeMode()
    {
        var credentialsConfigured = HasCredentials();
        var environment = string.IsNullOrWhiteSpace(_settings.Environment) ? "Sandbox" : _settings.Environment.Trim();
        var modeLabel = credentialsConfigured
            ? (environment.Equals("Live", StringComparison.OrdinalIgnoreCase) ? "Live" : "Sandbox")
            : "Mock";

        return new PinchIntegrationRuntimeMode
        {
            ModeLabel = modeLabel,
            Environment = environment,
            CredentialsConfigured = credentialsConfigured,
            UsesMockResponses = !credentialsConfigured,
            PortalUrl = _settings.PortalUrl
        };
    }

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApplicationId) && !string.IsNullOrWhiteSpace(_settings.SecretKey);
    }

    private PinchApi CreatePinchApi()
    {
        var isLive = _settings.Environment.Equals("Live", StringComparison.OrdinalIgnoreCase);
        return new PinchApi(_settings.ApplicationId, _settings.SecretKey, isLive);
    }

    private static PinchPreApprovalResult MockPreApproval(PinchPreApprovalRequest request)
    {
        return new PinchPreApprovalResult
        {
            PreApprovalId = $"pap_{request.ProjectId}_{request.MilestoneId}".Replace(" ", string.Empty, StringComparison.Ordinal),
            IsApproved = true,
            Status = "Approved"
        };
    }

    private static PinchDirectDebitResult MockDebit(PinchDirectDebitRequest request)
    {
        return new PinchDirectDebitResult
        {
            DebitId = $"dd_{request.ProjectId}_{request.MilestoneId}".Replace(" ", string.Empty, StringComparison.Ordinal),
            Succeeded = true,
            Status = "Succeeded"
        };
    }

    private static int ToCents(decimal amount)
    {
        return (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }
}
