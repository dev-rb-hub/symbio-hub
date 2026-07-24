using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Symbio.API.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var email = Request.Headers.TryGetValue("X-Test-Email", out var emailHeader)
            ? emailHeader.ToString()
            : "expert@example.com";

        var role = Request.Headers.TryGetValue("X-Test-Role", out var roleHeader)
            ? roleHeader.ToString()
            : "Expert";

        var hasMasterClaim = Request.Headers.TryGetValue("X-Test-Admin-Master", out var masterHeader)
            && string.Equals(masterHeader.ToString(), "true", StringComparison.OrdinalIgnoreCase);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role)
        };

        if (hasMasterClaim)
        {
            claims.Add(new Claim("symbio_admin_master", "true"));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
