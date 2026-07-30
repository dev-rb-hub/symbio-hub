using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Symbio.Infrastructure;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Symbio.API.Middleware;

public sealed class PinchSignatureValidationFilter : IAsyncResourceFilter
{
    private readonly PinchApiSettings _pinchApiSettings;

    public PinchSignatureValidationFilter(IConfiguration configuration)
    {
        _pinchApiSettings = PinchApiSettings.FromConfiguration(configuration);
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (!_pinchApiSettings.ValidateWebhookSignature)
        {
            context.HttpContext.Items[PinchWebhookTrustContext.ItemKey] = PinchWebhookTrustContext.BypassedState;
            await next();
            return;
        }

        var request = context.HttpContext.Request;
        request.EnableBuffering();
        request.Body.Position = 0;

        string rawBody;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
        }
        request.Body.Position = 0;

        if (!request.Headers.TryGetValue(_pinchApiSettings.WebhookSignatureHeader, out var headerValue))
        {
            context.HttpContext.Items[PinchWebhookTrustContext.ItemKey] = PinchWebhookTrustContext.RejectedState;
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Invalid webhook signature.",
                trustState = PinchWebhookTrustContext.RejectedState,
                reason = PinchSignatureValidationStatus.MissingSignatureHeader.ToString()
            });
            return;
        }

        var validationStatus = PinchSignatureVerifier.Validate(
            headerValue.ToString(),
            rawBody,
            _pinchApiSettings.WebhookSecret,
            _pinchApiSettings.WebhookSignatureVersion,
            _pinchApiSettings.WebhookToleranceSeconds);

        if (validationStatus != PinchSignatureValidationStatus.Valid)
        {
            context.HttpContext.Items[PinchWebhookTrustContext.ItemKey] = PinchWebhookTrustContext.RejectedState;
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Invalid webhook signature.",
                trustState = PinchWebhookTrustContext.RejectedState,
                reason = validationStatus.ToString()
            });
            return;
        }

        context.HttpContext.Items[PinchWebhookTrustContext.ItemKey] = PinchWebhookTrustContext.VerifiedState;

        await next();
    }
}

public static class PinchWebhookTrustContext
{
    public const string ItemKey = "PinchWebhookTrustState";
    public const string VerifiedState = "VerifiedSignature";
    public const string RejectedState = "RejectedSignature";
    public const string BypassedState = "SignatureValidationBypassed";
}

public enum PinchSignatureValidationStatus
{
    Valid,
    MissingData,
    MissingSignatureHeader,
    InvalidHeader,
    InvalidTimestamp,
    ToleranceExceeded,
    SignatureMismatch
}

public static class PinchSignatureVerifier
{
    public static PinchSignatureValidationStatus Validate(
        string signatureHeader,
        string rawBody,
        string webhookSecret,
        string signatureVersion,
        int toleranceSeconds)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader)
            || string.IsNullOrWhiteSpace(rawBody)
            || string.IsNullOrWhiteSpace(webhookSecret)
            || toleranceSeconds <= 0)
        {
            return PinchSignatureValidationStatus.MissingData;
        }

        if (!TryReadHeader(signatureHeader, signatureVersion, out var timestamp, out var signature))
        {
            return PinchSignatureValidationStatus.InvalidHeader;
        }

        if (!long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            return PinchSignatureValidationStatus.InvalidTimestamp;
        }

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        var age = DateTimeOffset.UtcNow - sentAt;
        if (age.Duration() > TimeSpan.FromSeconds(toleranceSeconds))
        {
            return PinchSignatureValidationStatus.ToleranceExceeded;
        }

        var signedPayload = $"{timestamp}.{rawBody}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();
        var normalizedSignature = signature.ToLowerInvariant();

        var signatureMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(normalizedSignature));

        return signatureMatches ? PinchSignatureValidationStatus.Valid : PinchSignatureValidationStatus.SignatureMismatch;
    }

    public static bool IsValid(
        string signatureHeader,
        string rawBody,
        string webhookSecret,
        string signatureVersion,
        int toleranceSeconds)
    {
        return Validate(signatureHeader, rawBody, webhookSecret, signatureVersion, toleranceSeconds) == PinchSignatureValidationStatus.Valid;
    }

    private static bool TryReadHeader(
        string signatureHeader,
        string signatureVersion,
        out string timestamp,
        out string signature)
    {
        timestamp = string.Empty;
        signature = string.Empty;

        var parts = signatureHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var pair = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                continue;
            }

            if (pair[0].Equals("t", StringComparison.OrdinalIgnoreCase))
            {
                timestamp = pair[1];
                continue;
            }

            if (pair[0].Equals(signatureVersion, StringComparison.OrdinalIgnoreCase))
            {
                signature = pair[1];
            }
        }

        return !string.IsNullOrWhiteSpace(timestamp) && !string.IsNullOrWhiteSpace(signature);
    }
}
