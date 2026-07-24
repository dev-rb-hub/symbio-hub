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

        if (!request.Headers.TryGetValue(_pinchApiSettings.WebhookSignatureHeader, out var headerValue)
            || !PinchSignatureVerifier.IsValid(
                headerValue.ToString(),
                rawBody,
                _pinchApiSettings.WebhookSecret,
                _pinchApiSettings.WebhookSignatureVersion,
                _pinchApiSettings.WebhookToleranceSeconds))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid webhook signature." });
            return;
        }

        await next();
    }
}

public static class PinchSignatureVerifier
{
    public static bool IsValid(
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
            return false;
        }

        if (!TryReadHeader(signatureHeader, signatureVersion, out var timestamp, out var signature))
        {
            return false;
        }

        if (!long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            return false;
        }

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        var age = DateTimeOffset.UtcNow - sentAt;
        if (age.Duration() > TimeSpan.FromSeconds(toleranceSeconds))
        {
            return false;
        }

        var signedPayload = $"{timestamp}.{rawBody}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();
        var normalizedSignature = signature.ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(normalizedSignature));
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
