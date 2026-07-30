namespace Symbio.Core.Models;

public sealed class PinchIntegrationRuntimeMode
{
    public string ModeLabel { get; init; } = "Mock";
    public string Environment { get; init; } = "Test";
    public bool CredentialsConfigured { get; init; }
    public bool UsesMockResponses { get; init; }
    public string BaseUri { get; init; } = string.Empty;
    public string AuthUri { get; init; } = string.Empty;
    public bool IsLive { get; init; }
}

public sealed class PinchSandboxVerificationResult
{
    public string ModeLabel { get; init; } = "Mock";
    public string Environment { get; init; } = "Test";
    public bool CredentialsConfigured { get; init; }
    public bool Connected { get; init; }
    public bool MerchantReadSucceeded { get; init; }
    public bool PayerListReadSucceeded { get; init; }
    public string BaseUri { get; init; } = string.Empty;
    public string AuthUri { get; init; } = string.Empty;
    public bool IsLive { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? MerchantName { get; init; }
    public string? FailureReason { get; init; }
    public string? MerchantReadErrorCode { get; init; }
    public string? MerchantReadErrorMessage { get; init; }
    public string? PayerListErrorCode { get; init; }
    public string? PayerListErrorMessage { get; init; }
    public int PayerListErrorCount { get; init; }
}
