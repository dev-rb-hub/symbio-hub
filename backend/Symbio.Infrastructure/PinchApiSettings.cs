using Microsoft.Extensions.Configuration;

namespace Symbio.Infrastructure;

public class PinchApiSettings
{
    public string BaseUrl { get; set; } = "https://api.getpinch.com.au";
    public string AuthBaseUrl { get; set; } = "https://auth.getpinch.com.au";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool ValidateWebhookSignature { get; set; }
    public string TokensPath { get; set; } = "/connect/token";
    public string ManagedMerchantsPath { get; set; } = "/test/merchants/managed";
    public string PreApprovalsPath { get; set; } = "/test/pre-approvals";
    public string DirectDebitsPath { get; set; } = "/test/payments";
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

        return new PinchApiSettings
        {
            BaseUrl = configuration["Pinch:BaseUrl"] ?? "https://api.getpinch.com.au",
            AuthBaseUrl = configuration["Pinch:AuthBaseUrl"] ?? "https://auth.getpinch.com.au",
            ApiKey = configuration["Pinch:ApiKey"] ?? string.Empty,
            ApiSecret = configuration["Pinch:ApiSecret"] ?? string.Empty,
            WebhookSecret = configuration["Pinch:WebhookSecret"] ?? string.Empty,
            ValidateWebhookSignature = bool.TryParse(configuration["Pinch:ValidateWebhookSignature"], out var validate) && validate,
            TokensPath = configuration["Pinch:TokensPath"] ?? "/connect/token",
            ManagedMerchantsPath = configuration["Pinch:ManagedMerchantsPath"] ?? "/test/merchants/managed",
            PreApprovalsPath = configuration["Pinch:PreApprovalsPath"] ?? "/test/pre-approvals",
            DirectDebitsPath = configuration["Pinch:DirectDebitsPath"] ?? "/test/payments",
            WebhookSignatureHeader = configuration["Pinch:WebhookSignatureHeader"] ?? "pinch-signature",
            WebhookSignatureVersion = configuration["Pinch:WebhookSignatureVersion"] ?? "v2",
            WebhookToleranceSeconds = tolerance
        };
    }
}