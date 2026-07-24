using Microsoft.Extensions.Configuration;

namespace Symbio.Infrastructure;

public class PinchApiSettings
{
    public string Environment { get; set; } = "Sandbox";
    public string BaseUrl { get; set; } = "https://api.getpinch.com.au";
    public string AuthBaseUrl { get; set; } = "https://auth.getpinch.com.au";
    public string PortalUrl { get; set; } = "https://sandbox.getpinch.com.au";
    public string ApplicationId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool ValidateWebhookSignature { get; set; }
    public string TokensPath { get; set; } = "/connect/token";
    public string ManagedMerchantsPath { get; set; } = "/test/merchants/managed";
    public string PreApprovalsPath { get; set; } = "/test/pre-approvals";
    public string DirectDebitsPath { get; set; } = "/test/payments";
    public string PlansPath { get; set; } = "/test/plans";
    public string SubscriptionsPath { get; set; } = "/test/subscriptions";
    public string WebhookSignatureHeader { get; set; } = "pinch-signature";
    public string WebhookSignatureVersion { get; set; } = "v2";
    public int WebhookToleranceSeconds { get; set; } = 300;
    public string TokenScope { get; set; } = string.Empty;

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
            BaseUrl = configuration["Pinch:BaseUrl"] ?? "https://api.getpinch.com.au",
            AuthBaseUrl = configuration["Pinch:AuthBaseUrl"] ?? "https://auth.getpinch.com.au",
            PortalUrl = configuration["Pinch:PortalUrl"] ?? "https://sandbox.getpinch.com.au",
            ApplicationId = string.IsNullOrWhiteSpace(applicationId) ? apiKey : applicationId,
            SecretKey = string.IsNullOrWhiteSpace(secretKey) ? apiSecret : secretKey,
            ApiKey = apiKey,
            ApiSecret = apiSecret,
            WebhookSecret = configuration["Pinch:WebhookSecret"] ?? string.Empty,
            ValidateWebhookSignature = bool.TryParse(configuration["Pinch:ValidateWebhookSignature"], out var validate) && validate,
            TokensPath = configuration["Pinch:TokensPath"] ?? "/connect/token",
            ManagedMerchantsPath = configuration["Pinch:ManagedMerchantsPath"] ?? "/test/merchants/managed",
            PreApprovalsPath = configuration["Pinch:PreApprovalsPath"] ?? "/test/pre-approvals",
            DirectDebitsPath = configuration["Pinch:DirectDebitsPath"] ?? "/test/payments",
            PlansPath = configuration["Pinch:PlansPath"] ?? "/test/plans",
            SubscriptionsPath = configuration["Pinch:SubscriptionsPath"] ?? "/test/subscriptions",
            WebhookSignatureHeader = configuration["Pinch:WebhookSignatureHeader"] ?? "pinch-signature",
            WebhookSignatureVersion = configuration["Pinch:WebhookSignatureVersion"] ?? "v2",
            WebhookToleranceSeconds = tolerance,
            TokenScope = configuration["Pinch:TokenScope"] ?? string.Empty
        };
    }
}