using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Symbio.API.Data;
using Symbio.API.Models;
using Xunit;

namespace Symbio.API.Tests;

public class EscrowOnboardingEndpointsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public EscrowOnboardingEndpointsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Status_Returns_NotStarted_For_New_Expert()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "newexpert@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Expert");

        var response = await client.GetAsync("/api/payments/onboarding/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("NotStarted", body.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Start_Then_SimulateComplete_Updates_Status_To_Verified()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "epic7expert@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Expert");

        var startResponse = await client.PostAsJsonAsync("/api/payments/onboarding/start", new { });
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        using var startBody = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());
        Assert.Equal("Pending", startBody.RootElement.GetProperty("status").GetString());

        var completeResponse = await client.PostAsJsonAsync("/api/payments/onboarding/simulate-complete", new { });
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        using var completeBody = JsonDocument.Parse(await completeResponse.Content.ReadAsStringAsync());
        Assert.Equal("Verified", completeBody.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Workbench_Milestone_Log_Requires_Verified_Escrow()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "expert@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Expert");

        var blockedResponse = await client.PostAsJsonAsync("/api/ExpertWorkbench/logs", new
        {
            deliveryAssignmentId = 1,
            message = "Milestone complete.",
            progressPercent = 100,
            status = "ReadyForSettlement",
            milestoneId = "M-100"
        });

        Assert.Equal(HttpStatusCode.PreconditionFailed, blockedResponse.StatusCode);

        var verifyResponse = await client.PostAsJsonAsync("/api/payments/onboarding/simulate-complete", new { });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var allowedResponse = await client.PostAsJsonAsync("/api/ExpertWorkbench/logs", new
        {
            deliveryAssignmentId = 1,
            message = "Milestone complete after escrow verification.",
            progressPercent = 100,
            status = "ReadyForSettlement",
            milestoneId = "M-100"
        });

        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task CanSettle_IsFalse_ThenTrue_When_Escrow_Verification_Changes()
    {
        const string milestoneId = "M-SETTLE-200";

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "admin@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var evidenceResponse = await client.PostAsJsonAsync("/api/CompletionEvidence/file-hash", new
        {
            milestoneId,
            epicId = "7",
            evidenceReferenceValue = "hash-escrow-gate-001",
            sourceCommitSha = "abc123",
            notes = "Settlement readiness test evidence",
            targetDeploymentUrl = "https://example.org/deploy/7"
        });

        Assert.Equal(HttpStatusCode.OK, evidenceResponse.StatusCode);

        var blockedSettleResponse = await client.GetAsync($"/api/CompletionEvidence/milestone/{milestoneId}/can-settle");
        Assert.Equal(HttpStatusCode.OK, blockedSettleResponse.StatusCode);

        using (var blockedBody = JsonDocument.Parse(await blockedSettleResponse.Content.ReadAsStringAsync()))
        {
            Assert.False(blockedBody.RootElement.GetProperty("canSettle").GetBoolean());
            Assert.False(blockedBody.RootElement.GetProperty("escrowVerified").GetBoolean());
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
            var profile = db.EscrowOnboardingProfiles.FirstOrDefault(item => item.ExpertEmail == "admin@example.com");

            if (profile == null)
            {
                db.EscrowOnboardingProfiles.Add(new EscrowOnboardingProfile
                {
                    ExpertEmail = "admin@example.com",
                    ProviderAccountId = "pinch-glassbox-admin-example-com-verified",
                    Status = "Verified",
                    OnboardingUrl = "https://connect.getpinch.com.au/glassbox/onboarding/pinch-glassbox-admin-example-com-verified",
                    LastSyncedAtUtc = DateTime.UtcNow,
                    OnboardedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                profile.Status = "Verified";
                profile.LastSyncedAtUtc = DateTime.UtcNow;
                profile.OnboardedAtUtc = DateTime.UtcNow;
                if (!profile.ProviderAccountId.Contains("verified", StringComparison.OrdinalIgnoreCase))
                {
                    profile.ProviderAccountId = $"{profile.ProviderAccountId}-verified";
                }
            }

            db.SaveChanges();
        }

        var readySettleResponse = await client.GetAsync($"/api/CompletionEvidence/milestone/{milestoneId}/can-settle");
        Assert.Equal(HttpStatusCode.OK, readySettleResponse.StatusCode);

        using var readyBody = JsonDocument.Parse(await readySettleResponse.Content.ReadAsStringAsync());
        Assert.True(readyBody.RootElement.GetProperty("canSettle").GetBoolean());
        Assert.True(readyBody.RootElement.GetProperty("escrowVerified").GetBoolean());
    }

    [Fact]
    public async Task Experts_Search_Alias_Returns_Paged_Results()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var response = await client.GetAsync("/api/experts/search?page=1&pageSize=5&query=developer");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(5, body.RootElement.GetProperty("pageSize").GetInt32());
        Assert.True(body.RootElement.GetProperty("results").ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task Pinch_Settlement_Webhook_Locks_Escrow_And_Applies_10_90_Split()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/webhooks/pinch-settlements", new
        {
            projectId = "demo-project-epic7-1",
            settlementStatus = "confirmed",
            amount = 1000m,
            currency = "AUD",
            providerReference = "pinch-ref-001"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("EscrowLocked", body.RootElement.GetProperty("state").GetString());
        Assert.Equal(100m, body.RootElement.GetProperty("platformFeeAmount").GetDecimal());
        Assert.Equal(900m, body.RootElement.GetProperty("contractorAmount").GetDecimal());
    }
}
