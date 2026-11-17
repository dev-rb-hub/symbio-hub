using System.Net;
using System.Text.Json;
using Xunit;

namespace Symbio.API.Tests;

public class TalentRegionalMatchingTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public TalentRegionalMatchingTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Radius_Search_Includes_Experts_Within_Range()
    {
        using var client = CreateSmeClient();

        // From Sydney (2000): Wollongong ~68km and Newcastle ~120km are within 150km.
        var response = await client.GetAsync("/api/talent?postcode=2000&radiusKm=150");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = Names(doc);

        Assert.Contains("Ava Chen", names);     // Newcastle
        Assert.Contains("Noah Patel", names);   // Wollongong
        Assert.DoesNotContain("Sophie Nguyen", names); // no structured location
    }

    [Fact]
    public async Task Radius_Search_Excludes_Experts_Out_Of_Range()
    {
        using var client = CreateSmeClient();

        // From Sydney (2000) within 100km: Wollongong (~68km) in, Newcastle (~120km) out.
        var response = await client.GetAsync("/api/talent?postcode=2000&radiusKm=100");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = Names(doc);

        Assert.Contains("Noah Patel", names);
        Assert.DoesNotContain("Ava Chen", names);
    }

    [Fact]
    public async Task Radius_Search_Excludes_Profiles_Missing_Location()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent?postcode=2000&radiusKm=5000");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = Names(doc);

        // Even with a very large radius, a profile without coordinates cannot be matched.
        Assert.DoesNotContain("Sophie Nguyen", names);
        Assert.Contains("Ava Chen", names);
    }

    [Fact]
    public async Task Radius_Search_With_Unknown_Postcode_Returns_No_Matches()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent?postcode=9999&radiusKm=150");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(doc.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task Radius_Search_Reports_Distance_For_Matches()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent?postcode=2000&radiusKm=150");
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var noah = doc.RootElement.EnumerateArray().Single(p => p.GetProperty("name").GetString() == "Noah Patel");
        var distance = noah.GetProperty("distanceKm").GetDouble();
        Assert.InRange(distance, 50, 90);
    }

    [Fact]
    public async Task Free_Text_Location_Search_Still_Works()
    {
        using var client = CreateSmeClient();

        var response = await client.GetAsync("/api/talent?location=Tamworth");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = Names(doc);

        Assert.Equal(new[] { "Sophie Nguyen" }, names);
    }

    private static List<string?> Names(JsonDocument doc) =>
        doc.RootElement.EnumerateArray().Select(p => p.GetProperty("name").GetString()).ToList();

    private HttpClient CreateSmeClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");
        return client;
    }
}
