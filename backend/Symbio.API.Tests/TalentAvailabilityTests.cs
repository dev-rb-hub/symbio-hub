using System.Net;
using System.Text.Json;
using Xunit;

namespace Symbio.API.Tests;

public class TalentAvailabilityTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public TalentAvailabilityTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Talent_Search_Reports_Active_Expired_And_Unset_Availability()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var profiles = doc.RootElement.EnumerateArray().ToList();

        var active = profiles.Single(p => p.GetProperty("name").GetString() == "Ava Chen");
        Assert.Equal("available", active.GetProperty("availabilityStatus").GetString());
        Assert.Equal(30, active.GetProperty("hoursPerWeek").GetInt32());

        var expired = profiles.Single(p => p.GetProperty("name").GetString() == "Noah Patel");
        Assert.Equal("expired", expired.GetProperty("availabilityStatus").GetString());
        Assert.Equal(JsonValueKind.Null, expired.GetProperty("hoursPerWeek").ValueKind);

        var unset = profiles.Single(p => p.GetProperty("name").GetString() == "Sophie Nguyen");
        Assert.Equal("unknown", unset.GetProperty("availabilityStatus").GetString());
        Assert.Equal(JsonValueKind.Null, unset.GetProperty("hoursPerWeek").ValueKind);
    }

    [Fact]
    public async Task Talent_Search_MinHours_Filters_Out_Low_Hours_Profiles()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent?minHoursPerWeek=25");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = doc.RootElement.EnumerateArray().Select(p => p.GetProperty("name").GetString()).ToList();

        Assert.Equal(new[] { "Ava Chen" }, names);
    }

    [Fact]
    public async Task Talent_Search_MinHours_Excludes_Expired_And_Unset_Availability()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent?minHoursPerWeek=15");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = doc.RootElement.EnumerateArray().Select(p => p.GetProperty("name").GetString()).ToList();

        Assert.Contains("Ava Chen", names);
        Assert.DoesNotContain("Noah Patel", names);
        Assert.DoesNotContain("Sophie Nguyen", names);
    }

    [Fact]
    public async Task Experts_Search_MinHours_Narrows_Total()
    {
        using var client = CreateSmeClient();

        var unfiltered = await client.GetAsync("/api/experts/search");
        using var unfilteredDoc = JsonDocument.Parse(await unfiltered.Content.ReadAsStringAsync());
        Assert.True(unfilteredDoc.RootElement.GetProperty("total").GetInt32() >= 3);

        var filtered = await client.GetAsync("/api/experts/search?minHoursPerWeek=25");
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);

        using var filteredDoc = JsonDocument.Parse(await filtered.Content.ReadAsStringAsync());
        Assert.Equal(1, filteredDoc.RootElement.GetProperty("total").GetInt32());
    }

    private HttpClient CreateSmeClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");
        return client;
    }
}
