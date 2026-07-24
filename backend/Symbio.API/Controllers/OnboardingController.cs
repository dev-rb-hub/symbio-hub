using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Models;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;

        public OnboardingController(SymbioDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public record OnboardingRequest(string Email, string CompanyName, string BusinessIdentifier, string ProfileSummary);

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Email,
                user.Role,
                user.CompanyName,
                user.BusinessIdentifier,
                user.ProfileSummary,
                user.OnboardingCompleted,
                user.OnboardedAt
            });
        }

        [HttpPost("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] OnboardingRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null)
            {
                return NotFound();
            }

            user.CompanyName = request.CompanyName;
            user.BusinessIdentifier = request.BusinessIdentifier;
            user.ProfileSummary = request.ProfileSummary;
            user.OnboardingCompleted = true;
            user.OnboardedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Profile updated" });
        }
    }
}
