using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Symbio.API.Data;
using Symbio.Core.Services;
using Symbio.Infrastructure;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    [AllowAnonymous]
    public class WebhooksController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;
        private readonly IPaymentSplitCalculator _paymentSplitCalculator;
        private readonly PinchApiSettings _pinchApiSettings;

        public WebhooksController(
            SymbioDbContext dbContext,
            IPaymentSplitCalculator paymentSplitCalculator,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _paymentSplitCalculator = paymentSplitCalculator;
            _pinchApiSettings = PinchApiSettings.FromConfiguration(configuration);
        }

        public record PinchSettlementWebhookRequest(string ProjectId, string SettlementStatus, decimal Amount, string Currency, string? ProviderReference);

        [HttpPost("pinch-settlements")]
        public async Task<IActionResult> HandlePinchSettlements()
        {
            var rawBody = await ReadRawBodyAsync();

            if (_pinchApiSettings.ValidateWebhookSignature)
            {
                if (!Request.Headers.TryGetValue(_pinchApiSettings.WebhookSignatureHeader, out var signatureHeader)
                    || !IsValidSignature(signatureHeader.ToString(), rawBody, _pinchApiSettings.WebhookSecret))
                {
                    return Unauthorized(new { message = "Invalid webhook signature." });
                }
            }

            var request = ParseSettlementRequest(rawBody);

            if (request == null || string.IsNullOrWhiteSpace(request.ProjectId) || request.Amount <= 0 || string.IsNullOrWhiteSpace(request.SettlementStatus))
            {
                return BadRequest(new { message = "ProjectId, SettlementStatus, and Amount are required." });
            }

            if (!request.SettlementStatus.Equals("confirmed", StringComparison.OrdinalIgnoreCase)
                && !request.SettlementStatus.Equals("succeeded", StringComparison.OrdinalIgnoreCase)
                && !request.SettlementStatus.Equals("escrow_locked", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { message = "Settlement webhook ignored for non-locking status." });
            }

            var state = await _dbContext.ProjectPaymentStateRecords
                .FirstOrDefaultAsync(item => item.ProjectId == request.ProjectId);

            if (state == null)
            {
                return NotFound(new { message = "Project payment state not found." });
            }

            var split = _paymentSplitCalculator.Calculate(request.Amount);
            state.State = "EscrowLocked";
            state.GrossAmount = split.GrossAmount;
            state.PlatformFeeAmount = split.PlatformFeeAmount;
            state.ContractorAmount = split.ContractorAmount;
            state.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();
            state.LastProviderReference = request.ProviderReference?.Trim();
            state.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                state.ProjectId,
                state.State,
                split.GrossAmount,
                split.PlatformFeeAmount,
                split.ContractorAmount,
                state.Currency,
                state.LastProviderReference,
                state.UpdatedAtUtc
            });
        }

        private async Task<string> ReadRawBodyAsync()
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;

            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            return rawBody;
        }

        private bool IsValidSignature(string signatureHeader, string rawBody, string webhookSecret)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(webhookSecret))
            {
                return false;
            }

            var parts = signatureHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string? timestamp = null;
            string? signature = null;

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

                if (pair[0].Equals(_pinchApiSettings.WebhookSignatureVersion, StringComparison.OrdinalIgnoreCase))
                {
                    signature = pair[1];
                }
            }

            if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            if (!long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
            {
                return false;
            }

            var sentAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            var age = DateTimeOffset.UtcNow - sentAt;
            if (age.Duration() > TimeSpan.FromSeconds(_pinchApiSettings.WebhookToleranceSeconds))
            {
                return false;
            }

            var signedPayload = $"{timestamp}.{rawBody}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var expected = Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
        }

        private static PinchSettlementWebhookRequest? ParseSettlementRequest(string rawBody)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
            {
                return null;
            }

            using var json = JsonDocument.Parse(rawBody);
            var root = json.RootElement;

            var payload = root;
            if (TryGetProperty(root, "Data", out var dataNode) || TryGetProperty(root, "data", out dataNode))
            {
                payload = dataNode;
            }

            var projectId = ReadString(payload, "ProjectId")
                ?? ReadString(payload, "projectId")
                ?? ReadNestedMetadataValue(root, "projectId");

            var settlementStatus = ReadString(payload, "SettlementStatus")
                ?? ReadString(payload, "settlementStatus")
                ?? ReadString(payload, "Status")
                ?? ReadString(payload, "status");

            var amount = ReadDecimal(payload, "Amount")
                ?? ReadDecimal(payload, "amount")
                ?? 0m;

            var currency = ReadString(payload, "Currency")
                ?? ReadString(payload, "currency")
                ?? "AUD";

            var providerReference = ReadString(payload, "ProviderReference")
                ?? ReadString(payload, "providerReference")
                ?? ReadString(payload, "Id")
                ?? ReadString(payload, "id");

            if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(settlementStatus) || amount <= 0)
            {
                return null;
            }

            return new PinchSettlementWebhookRequest(projectId, settlementStatus, amount, currency, providerReference);
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        private static string? ReadNestedMetadataValue(JsonElement root, string key)
        {
            if (TryGetProperty(root, "Metadata", out var metadata) || TryGetProperty(root, "metadata", out metadata))
            {
                return ReadString(metadata, key) ?? ReadString(metadata, key.ToLowerInvariant()) ?? ReadString(metadata, "ProjectId");
            }

            return null;
        }

        private static string? ReadString(JsonElement element, string propertyName)
        {
            if (!TryGetProperty(element, propertyName, out var value))
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        private static decimal? ReadDecimal(JsonElement element, string propertyName)
        {
            if (!TryGetProperty(element, propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var numeric))
            {
                return numeric;
            }

            if (value.ValueKind == JsonValueKind.String
                && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}