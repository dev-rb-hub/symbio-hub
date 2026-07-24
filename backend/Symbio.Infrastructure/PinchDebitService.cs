using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public sealed class PinchDebitService : IPinchDebitService
{
    private readonly HttpClient _httpClient;
    private readonly PinchApiSettings _settings;

    public PinchDebitService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = PinchApiSettings.FromConfiguration(configuration);
    }

    public async Task<PinchPreApprovalResult> CreatePreApprovalAsync(PinchPreApprovalRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockPreApproval(request);
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return MockPreApproval(request);
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{NormalizePath(_settings.PreApprovalsPath)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                amount = request.Amount,
                currency = request.Currency,
                reference = $"{request.ProjectId}:{request.MilestoneId}",
                bankAccountName = request.AccountName,
                bankAccountRoutingNumber = request.Bsb,
                bankAccountNumber = request.AccountNumber,
                customerEmail = request.CustomerEmail
            })
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return MockPreApproval(request);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var preApprovalId = ReadString(json.RootElement, "id")
            ?? ReadString(json.RootElement, "preApprovalId")
            ?? $"pap_{request.ProjectId}_{request.MilestoneId}";

        var status = ReadString(json.RootElement, "status") ?? "Approved";
        var approved = status.Equals("Approved", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Active", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);

        return new PinchPreApprovalResult
        {
            PreApprovalId = preApprovalId,
            IsApproved = approved,
            Status = approved ? "Approved" : status
        };
    }

    public async Task<PinchDirectDebitResult> ExecuteDirectDebitAsync(PinchDirectDebitRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasCredentials())
        {
            return MockDebit(request);
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return MockDebit(request);
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{NormalizePath(_settings.DirectDebitsPath)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                amount = request.Amount,
                currency = request.Currency,
                preApprovalId = request.PreApprovalId,
                reference = $"{request.ProjectId}:{request.MilestoneId}"
            })
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return MockDebit(request);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var debitId = ReadString(json.RootElement, "id")
            ?? ReadString(json.RootElement, "paymentId")
            ?? $"debit_{request.ProjectId}_{request.MilestoneId}";

        var status = ReadString(json.RootElement, "status") ?? "Succeeded";
        var succeeded = status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Processed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);

        return new PinchDirectDebitResult
        {
            DebitId = debitId,
            Status = status,
            Succeeded = succeeded,
            ErrorMessage = succeeded ? null : "Pinch API reported a non-success debit status."
        };
    }

    private bool HasCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApplicationId) && !string.IsNullOrWhiteSpace(_settings.SecretKey);
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var authBaseUrl = _settings.AuthBaseUrl.TrimEnd('/');
        var tokensUrl = $"{authBaseUrl}{NormalizePath(_settings.TokensPath)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, tokensUrl)
        {
            Content = new FormUrlEncodedContent(BuildTokenFormFields())
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return ReadString(json.RootElement, "access_token") ?? ReadString(json.RootElement, "token");
    }

    private static PinchPreApprovalResult MockPreApproval(PinchPreApprovalRequest request)
    {
        return new PinchPreApprovalResult
        {
            PreApprovalId = $"pap_{request.ProjectId}_{request.MilestoneId}".Replace(" ", string.Empty, StringComparison.Ordinal),
            IsApproved = true,
            Status = "Approved"
        };
    }

    private static PinchDirectDebitResult MockDebit(PinchDirectDebitRequest request)
    {
        return new PinchDirectDebitResult
        {
            DebitId = $"dd_{request.ProjectId}_{request.MilestoneId}".Replace(" ", string.Empty, StringComparison.Ordinal),
            Succeeded = true,
            Status = "Succeeded"
        };
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.StartsWith('/') ? path : $"/{path}";
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private Dictionary<string, string> BuildTokenFormFields()
    {
        var fields = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _settings.ApplicationId,
            ["client_secret"] = _settings.SecretKey
        };

        if (!string.IsNullOrWhiteSpace(_settings.TokenScope))
        {
            fields["scope"] = _settings.TokenScope.Trim();
        }

        return fields;
    }
}
