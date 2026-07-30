using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Symbio.API.Tests;

public class RetainerBillingTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public RetainerBillingTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Retainer_And_Add_Usage_Then_View_Control_Center()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var create = await client.PostAsJsonAsync("/api/retainers", new
        {
            projectId = "demo-project-epic10-1",
            milestoneId = "Maint-1",
            expertEmail = "expert@example.com",
            baseMonthlyAmount = 3000m,
            currency = "AUD",
            includedSupportHours = 12m,
            includedCloudUnits = 100m,
            overageRatePerHour = 120m,
            overageRatePerCloudUnit = 2.5m
        });

        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        using var createdBody = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var retainerId = createdBody.RootElement.GetProperty("id").GetInt32();

        var usage = await client.PostAsJsonAsync($"/api/retainers/{retainerId}/usage", new
        {
            supportHours = 18m,
            cloudUnits = 210m,
            periodStartUtc = DateTime.UtcNow.AddDays(-20),
            periodEndUtc = DateTime.UtcNow.AddDays(-1)
        });

        Assert.Equal(HttpStatusCode.OK, usage.StatusCode);

        var center = await client.GetAsync("/api/retainers/control-center");
        Assert.Equal(HttpStatusCode.OK, center.StatusCode);

        using var centerBody = JsonDocument.Parse(await center.Content.ReadAsStringAsync());
        Assert.True(centerBody.RootElement.GetProperty("retainers").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Pinch_Subscription_Webhook_Updates_Retainer_Status()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var create = await client.PostAsJsonAsync("/api/retainers", new
        {
            projectId = "demo-project-epic10-2",
            milestoneId = "Maint-2",
            expertEmail = "expert@example.com",
            baseMonthlyAmount = 2200m,
            currency = "AUD",
            includedSupportHours = 8m,
            includedCloudUnits = 40m,
            overageRatePerHour = 140m,
            overageRatePerCloudUnit = 3m
        });

        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        using var createdBody = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var providerSubscriptionId = createdBody.RootElement.GetProperty("providerSubscriptionId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(providerSubscriptionId));

        var webhook = await client.PostAsJsonAsync("/api/webhooks/pinch-subscriptions", new
        {
            providerSubscriptionId,
            status = "Paused",
            nextBillingAtUtc = DateTime.UtcNow.AddDays(30)
        });

        Assert.Equal(HttpStatusCode.OK, webhook.StatusCode);

        using var webhookBody = JsonDocument.Parse(await webhook.Content.ReadAsStringAsync());
        Assert.Equal("Paused", webhookBody.RootElement.GetProperty("status").GetString());
    }
}
