using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Symbio.API.Data;
using Symbio.API.Models;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SymbioDbContext _dbContext;

        public AuthController(IConfiguration configuration, SymbioDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public record RegisterRequest(string Email, string Password, string Role);
        public record LoginRequest(string Email, string Password);

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest();
            }

            if (request.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user != null)
            {
                if (user.PasswordHash != SeedData.HashPassword(request.Password))
                {
                    return Unauthorized();
                }

                if (!string.Equals(user.Role, request.Role, StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        message = $"This email is already registered as {user.Role}. Sign in with the existing role account or use a different email for {request.Role}.",
                        existingRole = user.Role
                    });
                }

                return Ok(new { token = GenerateJwtToken(user), role = user.Role });
            }

            var newUser = new User
            {
                Email = normalizedEmail,
                PasswordHash = SeedData.HashPassword(request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                OnboardingCompleted = false,
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new { token = GenerateJwtToken(newUser), role = newUser.Role });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest();
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);
            if (user == null || user.PasswordHash != SeedData.HashPassword(request.Password))
            {
                return Unauthorized();
            }

            return Ok(new { token = GenerateJwtToken(user), role = user.Role });
        }

        [HttpGet("verify-sme")]
        [Authorize(Policy = "RequireSmeRole")]
        public IActionResult VerifySme()
        {
            return Ok(new { message = "SME role verified" });
        }

        private string GenerateJwtToken(User user)
        {
            var keyString = _configuration["Jwt:Key"] ?? string.Empty;
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim("symbio_admin_master", "true"));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
