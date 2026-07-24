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
}
