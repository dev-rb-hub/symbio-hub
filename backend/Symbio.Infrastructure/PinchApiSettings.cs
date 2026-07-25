using Microsoft.Extensions.Configuration;

namespace Symbio.Infrastructure;

public class PinchApiSettings
{
    public string Environment { get; set; } = "Sandbox";
    public string PortalUrl { get; set; } = "https://sandbox.getpinch.com.au";
    public string ApplicationId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool ValidateWebhookSignature { get; set; }
    public string WebhookSignatureHeader { get; set; } = "pinch-signature";
    public string WebhookSignatureVersion { get; set; } = "v2";
    public int WebhookToleranceSeconds { get; set; } = 300;

    public static PinchApiSettings FromConfiguration(IConfiguration configuration)
    {
        var tolerance = 300;
        if (int.TryParse(configuration["Pinch:WebhookToleranceSeconds"], out var parsedTolerance) && parsedTolerance > 0)
        {
            tolerance = parsedTolerance;
        }

        var applicationId = configuration["Pinch:ApplicationId"] ?? string.Empty;
        var secretKey = configuration["Pinch:SecretKey"] ?? string.Empty;
        var apiKey = configuration["Pinch:ApiKey"] ?? string.Empty;
        var apiSecret = configuration["Pinch:ApiSecret"] ?? string.Empty;

        return new PinchApiSettings
        {
            Environment = configuration["Pinch:Environment"] ?? "Sandbox",
            PortalUrl = configuration["Pinch:PortalUrl"] ?? "https://sandbox.getpinch.com.au",
            ApplicationId = string.IsNullOrWhiteSpace(applicationId) ? apiKey : applicationId,
            SecretKey = string.IsNullOrWhiteSpace(secretKey) ? apiSecret : secretKey,
            ApiKey = apiKey,
            ApiSecret = apiSecret,
            WebhookSecret = configuration["Pinch:WebhookSecret"] ?? string.Empty,
            ValidateWebhookSignature = bool.TryParse(configuration["Pinch:ValidateWebhookSignature"], out var validate) && validate,
            WebhookSignatureHeader = configuration["Pinch:WebhookSignatureHeader"] ?? "pinch-signature",
            WebhookSignatureVersion = configuration["Pinch:WebhookSignatureVersion"] ?? "v2",
            WebhookToleranceSeconds = tolerance
        };
    }
}