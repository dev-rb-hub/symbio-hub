using System.Text.Json.Serialization;

namespace Symbio.API.Models;

public sealed class PinchWebhookEnvelope
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("EventDate")]
    public DateTimeOffset EventDate { get; set; }

    [JsonPropertyName("Metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("Data")]
    public PinchWebhookData Data { get; set; } = new();
}

public sealed class PinchWebhookData
{
    [JsonPropertyName("ProjectId")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("SettlementStatus")]
    public string? SettlementStatus { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("Amount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Amount { get; set; }

    [JsonPropertyName("Currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("ProviderReference")]
    public string? ProviderReference { get; set; }

    [JsonPropertyName("Id")]
    public string? Id { get; set; }
}

public sealed record PinchSettlementWebhookRequest(
    string ProjectId,
    string SettlementStatus,
    decimal Amount,
    string Currency,
    string? ProviderReference);

public static class PinchWebhookMapper
{
    public static bool TryMapSettlementRequest(PinchWebhookEnvelope? envelope, out PinchSettlementWebhookRequest? request)
    {
        request = null;

        if (envelope == null || string.IsNullOrWhiteSpace(envelope.Id) || string.IsNullOrWhiteSpace(envelope.Type))
        {
            return false;
        }

        var projectId = envelope.Data.ProjectId;
        if (string.IsNullOrWhiteSpace(projectId) && envelope.Metadata is { Count: > 0 })
        {
            if (!envelope.Metadata.TryGetValue("ProjectId", out projectId))
            {
                envelope.Metadata.TryGetValue("projectId", out projectId);
            }
        }

        var settlementStatus = string.IsNullOrWhiteSpace(envelope.Data.SettlementStatus)
            ? envelope.Data.Status
            : envelope.Data.SettlementStatus;

        var amount = envelope.Data.Amount;
        var currency = string.IsNullOrWhiteSpace(envelope.Data.Currency) ? "AUD" : envelope.Data.Currency;
        var providerReference = string.IsNullOrWhiteSpace(envelope.Data.ProviderReference)
            ? envelope.Data.Id ?? envelope.Id
            : envelope.Data.ProviderReference;

        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(settlementStatus) || amount <= 0)
        {
            return false;
        }

        request = new PinchSettlementWebhookRequest(
            projectId.Trim(),
            settlementStatus.Trim(),
            amount,
            currency.Trim().ToUpperInvariant(),
            providerReference?.Trim());

        return true;
    }
}