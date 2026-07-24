using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Symbio.API.Tests;

public class AdminOperationsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AdminOperationsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_Endpoints_Are_Blocked_Without_Master_Claim()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "admin@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var response = await client.GetAsync("/api/admin/telemetry/global");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Endpoints_Work_With_Master_Claim_And_Persist_Override()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "admin@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-Admin-Master", "true");

        var telemetry = await client.GetAsync("/api/admin/telemetry/global");
        Assert.Equal(HttpStatusCode.OK, telemetry.StatusCode);

        var upsert = await client.PostAsJsonAsync("/api/admin/overrides/safety-settings", new
        {
            settingKey = "platform.alertThreshold",
            settingValue = "3"
        });
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        var settings = await client.GetAsync("/api/admin/overrides/safety-settings");
        Assert.Equal(HttpStatusCode.OK, settings.StatusCode);

        using var settingsBody = JsonDocument.Parse(await settings.Content.ReadAsStringAsync());
        var any = settingsBody.RootElement.EnumerateArray().Any(item =>
            item.GetProperty("settingKey").GetString() == "platform.alertThreshold"
            && item.GetProperty("settingValue").GetString() == "3");

        Assert.True(any);
    }
}
