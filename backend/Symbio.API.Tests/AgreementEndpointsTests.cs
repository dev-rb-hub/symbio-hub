using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Symbio.API.Tests;

public class AgreementEndpointsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AgreementEndpointsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Upsert_Agreement_Uses_One_Record_Per_Project()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var first = await client.PostAsJsonAsync("/api/agreements/upsert", new
        {
            projectId = "project-unique-001",
            projectTitle = "Project One",
            milestoneId = "Kickoff",
            amount = 9000,
            currency = "AUD"
        });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/agreements/upsert", new
        {
            projectId = "project-unique-001",
            projectTitle = "Project One Updated",
            milestoneId = "Kickoff",
            amount = 9100,
            currency = "AUD"
        });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var list = await client.GetAsync("/api/agreements?includePending=true&includeClosed=true&search=project-unique-001");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        using var doc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetProperty("count").GetInt32());

        var only = doc.RootElement.GetProperty("agreements").EnumerateArray().Single();
        Assert.Equal("Project One Updated", only.GetProperty("projectTitle").GetString());
        Assert.Equal(9100m, only.GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task Agreement_List_Is_Filtered_By_User_Role_Relationship()
    {
        using var smeClient = _factory.CreateClient();
        smeClient.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        smeClient.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var smeUpsert = await smeClient.PostAsJsonAsync("/api/agreements/upsert", new
        {
            projectId = "project-sme-visible-001",
            projectTitle = "SME Visible",
            milestoneId = "Kickoff",
            amount = 10000,
            currency = "AUD"
        });
        Assert.Equal(HttpStatusCode.OK, smeUpsert.StatusCode);

        using var expertClient = _factory.CreateClient();
        expertClient.DefaultRequestHeaders.Add("X-Test-Email", "expert@example.com");
        expertClient.DefaultRequestHeaders.Add("X-Test-Role", "Expert");

        using var smeTwoClient = _factory.CreateClient();
        smeTwoClient.DefaultRequestHeaders.Add("X-Test-Email", "sme-two@example.com");
        smeTwoClient.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var smeTwoUpsert = await smeTwoClient.PostAsJsonAsync("/api/agreements/upsert", new
        {
            projectId = "project-expert-visible-001",
            projectTitle = "Expert Visible",
            milestoneId = "Kickoff",
            amount = 11000,
            currency = "AUD"
        });
        Assert.Equal(HttpStatusCode.OK, smeTwoUpsert.StatusCode);

        var expertUpsert = await expertClient.PostAsJsonAsync("/api/agreements/upsert", new
        {
            projectId = "project-expert-visible-001",
            projectTitle = "Expert Visible",
            milestoneId = "Kickoff",
            amount = 11000,
            currency = "AUD"
        });
        Assert.Equal(HttpStatusCode.OK, expertUpsert.StatusCode);

        var smeList = await smeClient.GetAsync("/api/agreements?includePending=true&includeClosed=true");
        Assert.Equal(HttpStatusCode.OK, smeList.StatusCode);

        using var smeDoc = JsonDocument.Parse(await smeList.Content.ReadAsStringAsync());
        var smeProjects = smeDoc.RootElement.GetProperty("agreements").EnumerateArray().Select(item => item.GetProperty("projectId").GetString()).ToList();

        Assert.Contains("project-sme-visible-001", smeProjects);
        Assert.DoesNotContain("project-expert-visible-001", smeProjects);

        var expertList = await expertClient.GetAsync("/api/agreements?includePending=true&includeClosed=true");
        Assert.Equal(HttpStatusCode.OK, expertList.StatusCode);

        using var expertDoc = JsonDocument.Parse(await expertList.Content.ReadAsStringAsync());
        var expertProjects = expertDoc.RootElement.GetProperty("agreements").EnumerateArray().Select(item => item.GetProperty("projectId").GetString()).ToList();

        Assert.Contains("project-expert-visible-001", expertProjects);
        Assert.DoesNotContain("project-sme-visible-001", expertProjects);
    }
}
