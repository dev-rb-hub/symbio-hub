using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Symbio.API.Tests;

public class PaymentsFlowTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public PaymentsFlowTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PreApproval_Capture_Stores_Approved_State()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var response = await client.PostAsJsonAsync("/api/payments/pre-approvals", new
        {
            projectId = "demo-project-epic7-1",
            milestoneId = "Kickoff",
            accountName = "Coastal SME Services",
            bsb = "123-456",
            accountNumber = "123456789",
            amount = 9500m,
            currency = "AUD"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Approved", body.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task SignOff_Queues_Direct_Debit_When_PreApproval_Approved()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Email", "sme@example.com");
        client.DefaultRequestHeaders.Add("X-Test-Role", "SME");

        var preApproval = await client.PostAsJsonAsync("/api/payments/pre-approvals", new
        {
            projectId = "demo-project-epic7-1",
            milestoneId = "Kickoff",
            accountName = "Coastal SME Services",
            bsb = "123-456",
            accountNumber = "123456789",
            amount = 9500m,
            currency = "AUD"
        });

        Assert.Equal(HttpStatusCode.OK, preApproval.StatusCode);

        var signOff = await client.PostAsJsonAsync("/api/payments/milestones/sign-off", new
        {
            projectId = "demo-project-epic7-1",
            milestoneId = "Kickoff"
        });

        Assert.Equal(HttpStatusCode.Accepted, signOff.StatusCode);

        using var body = JsonDocument.Parse(await signOff.Content.ReadAsStringAsync());
        Assert.Equal("Pending", body.RootElement.GetProperty("status").GetString());
    }
}
