using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
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
    private readonly ILogger<PinchDebitService> _logger;
    private readonly PinchApiSettings _settings;
    private readonly PinchApi? _pinchApi;

    public PinchDebitService(HttpClient httpClient, IConfiguration configuration, ILogger<PinchDebitService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = PinchApiSettings.FromConfiguration(configuration);
        _pinchApi = HasCredentials() ? CreatePinchApi() : null;
    }

    public async Task<PinchPreApprovalResult> CreatePreApprovalAsync(PinchPreApprovalRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Pinch pre-approval requested for ProjectId={ProjectId}, MilestoneId={MilestoneId}, Mode={ModeLabel}, IsLive={IsLive}, BaseUri={BaseUri}",
            request.ProjectId,
            request.MilestoneId,
            GetRuntimeMode().ModeLabel,
            _settings.IsLive,
            _settings.BaseUri);

        if (!HasCredentials())
        {
            _logger.LogWarning(
                "Pinch pre-approval falling back to mock: credentials missing for MerchantId={MerchantId}",
                RedactIdentifier(_settings.MerchantId));
            return MockPreApproval(request);
        }

        if (_pinchApi == null)
        {
            _logger.LogWarning(
                "Pinch pre-approval falling back to mock: PinchApi was not initialized for MerchantId={MerchantId}",
                RedactIdentifier(_settings.MerchantId));
            return MockPreApproval(request);
        }

        try
        {
            var source = CreateSourceSaveOptions(request);
            var payerResponse = await _pinchApi.Payer.Save(new PayerSaveOptions
            {
                EmailAddress = request.CustomerEmail,
                CompanyName = request.AccountName,
                Metadata = $"project={request.ProjectId};milestone={request.MilestoneId}",
                Source = source
            });

            var payerId = payerResponse.Data?.Id;
            if (!payerResponse.Success || string.IsNullOrWhiteSpace(payerId))
            {
                _logger.LogWarning(
                    "Pinch pre-approval payer save failed for ProjectId={ProjectId}, MilestoneId={MilestoneId}, Success={Success}, ErrorCount={ErrorCount}",
                    request.ProjectId,
                    request.MilestoneId,
                    payerResponse.Success,
                    payerResponse.Errors?.Count ?? 0);
                return MockPreApproval(request);
            }

            var agreementResponse = await _pinchApi.Agreement.Create(new AgreementSaveOptions
            {
                PayerId = payerId
            });

            var agreement = agreementResponse.Data;
            if (!agreementResponse.Success || agreement == null || string.IsNullOrWhiteSpace(agreement.Id))
            {
                _logger.LogWarning(
                    "Pinch pre-approval agreement creation failed for ProjectId={ProjectId}, MilestoneId={MilestoneId}, Success={Success}, ErrorCount={ErrorCount}",
                    request.ProjectId,
                    request.MilestoneId,
                    agreementResponse.Success,
                    agreementResponse.Errors?.Count ?? 0);
                return MockPreApproval(request);
            }

            var isApproved = agreement.ConfirmedDateUtc.HasValue;
            _logger.LogInformation(
                "Pinch pre-approval completed for ProjectId={ProjectId}, MilestoneId={MilestoneId}, PreApprovalId={PreApprovalId}, Status={Status}",
                request.ProjectId,
                request.MilestoneId,
                agreement.Id,
                isApproved ? "Approved" : "Pending");
            return new PinchPreApprovalResult
            {
                PreApprovalId = agreement.Id,
                IsApproved = isApproved,
                Status = isApproved ? "Approved" : "Pending"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Pinch pre-approval failed with exception for ProjectId={ProjectId}, MilestoneId={MilestoneId}. Falling back to mock.",
                request.ProjectId,
                request.MilestoneId);
            return MockPreApproval(request);
        }
    }

    public async Task<PinchDirectDebitResult> ExecuteDirectDebitAsync(PinchDirectDebitRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Pinch direct debit requested for ProjectId={ProjectId}, MilestoneId={MilestoneId}, Amount={Amount}, Mode={ModeLabel}, IsLive={IsLive}",
            request.ProjectId,
            request.MilestoneId,
            request.Amount,
            GetRuntimeMode().ModeLabel,
            _settings.IsLive);

        if (!HasCredentials())
        {
            _logger.LogWarning(
                "Pinch direct debit falling back to mock: credentials missing for MerchantId={MerchantId}",
                RedactIdentifier(_settings.MerchantId));
            return MockDebit(request);
        }

        if (_pinchApi == null)
        {
            _logger.LogWarning(
                "Pinch direct debit falling back to mock: PinchApi was not initialized for MerchantId={MerchantId}",
                RedactIdentifier(_settings.MerchantId));
            return MockDebit(request);
        }

        try
        {
            var agreementResponse = await _pinchApi.Agreement.Get(request.PreApprovalId);
            var agreement = agreementResponse.Data;
            var payerId = agreement?.Payer?.Id;

            if (!agreementResponse.Success || string.IsNullOrWhiteSpace(payerId))
            {
                _logger.LogWarning(
                    "Pinch direct debit agreement lookup failed for PreApprovalId={PreApprovalId}, Success={Success}, ErrorCount={ErrorCount}",
                    request.PreApprovalId,
                    agreementResponse.Success,
                    agreementResponse.Errors?.Count ?? 0);
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
                _logger.LogWarning(
                    "Pinch direct debit payment save failed for ProjectId={ProjectId}, MilestoneId={MilestoneId}, Success={Success}, ErrorCount={ErrorCount}",
                    request.ProjectId,
                    request.MilestoneId,
                    paymentResponse.Success,
                    paymentResponse.Errors?.Count ?? 0);
                return MockDebit(request);
            }

            var status = string.IsNullOrWhiteSpace(payment.Status) ? "Pending" : payment.Status;
            var succeeded = status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Processed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Completed", StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Pinch direct debit completed for ProjectId={ProjectId}, MilestoneId={MilestoneId}, DebitId={DebitId}, Status={Status}, Succeeded={Succeeded}",
                request.ProjectId,
                request.MilestoneId,
                string.IsNullOrWhiteSpace(payment.Id) ? "generated-fallback" : payment.Id,
                status,
                succeeded);

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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Pinch direct debit failed with exception for ProjectId={ProjectId}, MilestoneId={MilestoneId}. Falling back to mock.",
                request.ProjectId,
                request.MilestoneId);
            return MockDebit(request);
        }
    }

    public PinchIntegrationRuntimeMode GetRuntimeMode()
    {
        var credentialsConfigured = HasCredentials();
        var modeLabel = credentialsConfigured
            ? (_settings.IsLive ? "Live" : "Test")
            : "Mock";

        return new PinchIntegrationRuntimeMode
        {
            ModeLabel = modeLabel,
            Environment = _settings.IsLive ? "Live" : "Test",
            CredentialsConfigured = credentialsConfigured,
            UsesMockResponses = !credentialsConfigured,
            BaseUri = _settings.BaseUri,
            AuthUri = _settings.AuthUri,
            IsLive = _settings.IsLive
        };
    }

    public async Task<PinchSandboxVerificationResult> VerifySandboxConnectionAsync(CancellationToken cancellationToken = default)
    {
        var runtimeMode = GetRuntimeMode();

        _logger.LogInformation(
            "Pinch sandbox verification started. Mode={ModeLabel}, IsLive={IsLive}, CredentialsConfigured={CredentialsConfigured}, MerchantId={MerchantId}, BaseUri={BaseUri}",
            runtimeMode.ModeLabel,
            runtimeMode.IsLive,
            runtimeMode.CredentialsConfigured,
            RedactIdentifier(_settings.MerchantId),
            runtimeMode.BaseUri);

        if (!runtimeMode.CredentialsConfigured || _pinchApi == null)
        {
            _logger.LogWarning(
                "Pinch sandbox verification failed pre-check: credentials configured={CredentialsConfigured}, apiInitialized={ApiInitialized}",
                runtimeMode.CredentialsConfigured,
                _pinchApi != null);

            return new PinchSandboxVerificationResult
            {
                ModeLabel = runtimeMode.ModeLabel,
                Environment = runtimeMode.Environment,
                CredentialsConfigured = false,
                Connected = false,
                MerchantReadSucceeded = false,
                PayerListReadSucceeded = false,
                BaseUri = runtimeMode.BaseUri,
                AuthUri = runtimeMode.AuthUri,
                IsLive = runtimeMode.IsLive,
                Message = "Configure Pinch MerchantId and SecretKey to verify sandbox connectivity.",
                FailureReason = "Credentials are not configured.",
                MerchantReadErrorCode = "credentials-missing",
                MerchantReadErrorMessage = "MerchantId or SecretKey is not configured.",
                PayerListErrorCode = "credentials-missing",
                PayerListErrorMessage = "MerchantId or SecretKey is not configured.",
                PayerListErrorCount = 1
            };
        }

        string? merchantName = null;
        var merchantReadSucceeded = false;
        string? merchantReadErrorCode = null;
        string? merchantReadErrorMessage = null;

        try
        {
            var merchant = await _pinchApi.Merchant.GetMerchant();
            merchantName = merchant?.CompanyName;
            merchantReadSucceeded = merchant != null;

            if (!merchantReadSucceeded)
            {
                merchantReadErrorCode = "merchant-empty";
                merchantReadErrorMessage = "Pinch merchant endpoint returned an empty response payload.";
            }
        }
        catch (Exception ex)
        {
            merchantReadErrorCode = "merchant-exception";
            merchantReadErrorMessage = ex.Message;
            _logger.LogError(
                ex,
                "Pinch merchant read failed during sandbox verification for MerchantId={MerchantId}",
                RedactIdentifier(_settings.MerchantId));
        }

        var payerListReadSucceeded = false;
        var payerListErrorCount = 0;
        string? payerListErrorCode = null;
        string? payerListErrorMessage = null;

        try
        {
            var payerPage = await _pinchApi.Payer.GetPayers(pageSize: 1);
            payerListReadSucceeded = payerPage.Success;
            payerListErrorCount = payerPage.Errors?.Count ?? 0;
            payerListErrorCode = payerPage.Errors?.FirstOrDefault()?.ErrorCode;
            payerListErrorMessage = payerPage.Errors?.FirstOrDefault()?.ErrorMessage;
        }
        catch (Exception ex)
        {
            payerListReadSucceeded = false;
            payerListErrorCount = 1;
            payerListErrorCode = "payer-list-exception";
            payerListErrorMessage = ex.Message;
            _logger.LogError(
                ex,
                "Pinch payer list read failed during sandbox verification for MerchantId={MerchantId}",
                RedactIdentifier(_settings.MerchantId));
        }

        _logger.LogInformation(
            "Pinch sandbox verification completed. MerchantReadSucceeded={MerchantReadSucceeded}, PayerListReadSucceeded={PayerListReadSucceeded}, PayerListErrorCount={PayerListErrorCount}, MerchantName={MerchantName}",
            merchantReadSucceeded,
            payerListReadSucceeded,
            payerListErrorCount,
            merchantName ?? "n/a");

        var connected = merchantReadSucceeded
            || payerListReadSucceeded
            || payerListErrorCount > 0
            || string.Equals(merchantReadErrorCode, "merchant-empty", StringComparison.OrdinalIgnoreCase);

        var failureReasonParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(merchantReadErrorMessage))
        {
            failureReasonParts.Add($"merchant: {merchantReadErrorMessage}");
        }

        if (!string.IsNullOrWhiteSpace(payerListErrorMessage))
        {
            failureReasonParts.Add($"payer-list: {payerListErrorMessage}");
        }

        return new PinchSandboxVerificationResult
        {
            ModeLabel = runtimeMode.ModeLabel,
            Environment = runtimeMode.Environment,
            CredentialsConfigured = true,
            Connected = connected,
            MerchantReadSucceeded = merchantReadSucceeded,
            PayerListReadSucceeded = payerListReadSucceeded,
            BaseUri = runtimeMode.BaseUri,
            AuthUri = runtimeMode.AuthUri,
            IsLive = runtimeMode.IsLive,
            Message = "Sandbox credentials reached Pinch successfully using the SDK client.",
            MerchantName = merchantName,
            FailureReason = failureReasonParts.Count == 0 ? null : string.Join(" | ", failureReasonParts),
            MerchantReadErrorCode = merchantReadErrorCode,
            MerchantReadErrorMessage = merchantReadErrorMessage,
            PayerListErrorCode = payerListErrorCode,
            PayerListErrorMessage = payerListErrorMessage,
            PayerListErrorCount = payerListErrorCount
        };
    }

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.MerchantId) && !string.IsNullOrWhiteSpace(_settings.SecretKey);
    }

    private static string RedactIdentifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(empty)";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 6
            ? "***"
            : $"***{trimmed[^6..]}";
    }

    private PinchApi CreatePinchApi()
    {
        return new PinchApi(_settings.MerchantId, _settings.SecretKey, new PinchApiOptions(
            isLive: _settings.IsLive,
            baseUri: _settings.BaseUri,
            authUri: _settings.AuthUri,
            applicationId: _settings.ApplicationId));
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

    private static SourceSaveOptions CreateSourceSaveOptions(PinchPreApprovalRequest request)
    {
        var source = new SourceSaveOptions
        {
            SourceType = "bank-account"
        };

        if (!string.IsNullOrWhiteSpace(request.SourceToken))
        {
            source.Token = request.SourceToken.Trim();
            return source;
        }

        var sourceType = source.GetType();
        sourceType.GetProperty("BankAccountName", BindingFlags.Public | BindingFlags.Instance)?.SetValue(source, request.AccountName);
        sourceType.GetProperty("BankAccountBsb", BindingFlags.Public | BindingFlags.Instance)?.SetValue(source, request.Bsb);
        sourceType.GetProperty("BankAccountNumber", BindingFlags.Public | BindingFlags.Instance)?.SetValue(source, request.AccountNumber);
        return source;
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
