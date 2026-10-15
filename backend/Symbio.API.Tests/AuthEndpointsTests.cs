using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Symbio.API.Data;
using Symbio.API.Models;
using Symbio.API.Services;
using Xunit;

namespace Symbio.API.Tests;

public sealed class AuthEndpointsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AuthEndpointsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    private void AddUser(string email, bool isActive)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        db.Users.Add(new User
        {
            Email = email,
            PasswordHash = passwords.Hash("password123"),
            Role = "SME",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task Register_ForDeactivatedAccount_ReturnsUnauthorized()
    {
        AddUser("deactivated-register@example.com", isActive: false);
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = "deactivated-register@example.com", password = "password123", role = "SME" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ForDeactivatedAccount_ReturnsUnauthorized()
    {
        AddUser("deactivated-login@example.com", isActive: false);
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "deactivated-login@example.com", password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_ForActiveAccountWithCorrectPassword_ReturnsToken()
    {
        AddUser("active-register@example.com", isActive: true);
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = "active-register@example.com", password = "password123", role = "SME" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
