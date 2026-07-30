using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Symbio.API.Tests;

public sealed class WebhookSecurityTests : IClassFixture<SignedWebhookApiTestFactory>
{
    private readonly SignedWebhookApiTestFactory _factory;

    public WebhookSecurityTests(SignedWebhookApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PinchSettlement_WithValidSignature_IsAccepted_AndMarkedVerified()
    {
        using var client = _factory.CreateClient();

        var payload = JsonSerializer.Serialize(new
        {
            id = "evt_signed_ok_001",
            type = "transfer",
            eventDate = DateTimeOffset.UtcNow,
            metadata = new
            {
                projectId = "demo-project-epic7-1"
            },
            data = new
            {
                settlementStatus = "confirmed",
                amount = 1000m,
                currency = "AUD",
                providerReference = "pinch-signed-ok-001"
            }
        });

        using var request = BuildSignedRequest(payload);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("VerifiedSignature", body.RootElement.GetProperty("trustState").GetString());
        Assert.Equal("Valid", body.RootElement.GetProperty("trustReason").GetString());
    }

    [Fact]
    public async Task PinchSettlement_ReplayedSignature_IsRejected()
    {
        using var client = _factory.CreateClient();

        var payload = JsonSerializer.Serialize(new
        {
            id = "evt_replay_001",
            type = "transfer",
            eventDate = DateTimeOffset.UtcNow,
            metadata = new
            {
                projectId = "demo-project-epic7-1"
            },
            data = new
            {
                settlementStatus = "confirmed",
                amount = 1200m,
                currency = "AUD",
                providerReference = "pinch-replay-001"
            }
        });

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signatureHeader = BuildSignatureHeader(payload, timestamp);

        using var firstRequest = BuildSignedRequest(payload, signatureHeader);
        var firstResponse = await client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var replayRequest = BuildSignedRequest(payload, signatureHeader);
        var replayResponse = await client.SendAsync(replayRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        using var replayBody = JsonDocument.Parse(await replayResponse.Content.ReadAsStringAsync());
        Assert.Equal("RejectedSignature", replayBody.RootElement.GetProperty("trustState").GetString());
        Assert.Equal("ReplayDetected", replayBody.RootElement.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task PinchSettlement_MissingSignatureHeader_IsRejected()
    {
        using var client = _factory.CreateClient();

        var payload = JsonSerializer.Serialize(new
        {
            id = "evt_missing_header_001",
            type = "transfer",
            eventDate = DateTimeOffset.UtcNow,
            metadata = new
            {
                projectId = "demo-project-epic7-1"
            },
            data = new
            {
                settlementStatus = "confirmed",
                amount = 1250m,
                currency = "AUD",
                providerReference = "pinch-missing-header-001"
            }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pinch-settlements")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("RejectedSignature", body.RootElement.GetProperty("trustState").GetString());
        Assert.Equal("MissingSignatureHeader", body.RootElement.GetProperty("reason").GetString());
    }

    private static HttpRequestMessage BuildSignedRequest(string payload, string? signatureHeader = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var computedHeader = signatureHeader ?? BuildSignatureHeader(payload, timestamp);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pinch-settlements")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("pinch-signature", computedHeader);
        return request;
    }

    private static string BuildSignatureHeader(string payload, long timestamp)
    {
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SignedWebhookApiTestFactory.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var signatureHex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"t={timestamp},v2={signatureHex}";
    }
}

public sealed class SignedWebhookApiTestFactory : ApiTestFactory
{
    public const string WebhookSecret = "signed-webhook-test-secret";

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Pinch:ValidateWebhookSignature"] = "true",
                ["Pinch:WebhookSecret"] = WebhookSecret,
                ["Pinch:WebhookSignatureHeader"] = "pinch-signature",
                ["Pinch:WebhookSignatureVersion"] = "v2",
                ["Pinch:WebhookToleranceSeconds"] = "300"
            };

            configBuilder.AddInMemoryCollection(overrides);
        });
    }
}
