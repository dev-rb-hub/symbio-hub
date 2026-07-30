namespace Symbio.Core.Models;

public sealed class PinchIntegrationRuntimeMode
{
    public string ModeLabel { get; init; } = "Mock";
    public string Environment { get; init; } = "Sandbox";
    public bool CredentialsConfigured { get; init; }
    public bool UsesMockResponses { get; init; }
    public string PortalUrl { get; init; } = string.Empty;
}
