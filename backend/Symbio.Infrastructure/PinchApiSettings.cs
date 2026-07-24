namespace Symbio.Infrastructure;

public class PinchApiSettings
{
    public string BaseUrl { get; set; } = "https://api.getpinch.com.au";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool ValidateWebhookSignature { get; set; }
    public string TokensPath { get; set; } = "/tokens";
    public string ManagedMerchantsPath { get; set; } = "/managed-merchants";
}