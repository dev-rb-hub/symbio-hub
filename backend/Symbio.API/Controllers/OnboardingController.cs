using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symbio.API.Data;
using Symbio.API.Models;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;
        private readonly ITalentDiscoveryRepository _talentDiscoveryRepository;

        public OnboardingController(SymbioDbContext dbContext, ITalentDiscoveryRepository talentDiscoveryRepository)
        {
            _dbContext = dbContext;
            _talentDiscoveryRepository = talentDiscoveryRepository;
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

            if (string.Equals(user.Role, "Expert", StringComparison.OrdinalIgnoreCase))
            {
                await _talentDiscoveryRepository.UpsertTalentProfileAsync(MapToTalentProfile(user));
            }

            return Ok(new { message = "Profile updated" });
        }

        private static TalentProfile MapToTalentProfile(User user)
        {
            return new TalentProfile
            {
                Id = user.Email,
                Name = string.IsNullOrWhiteSpace(user.CompanyName) ? user.Email : user.CompanyName,
                CompanyName = user.CompanyName,
                Email = user.Email,
                Role = user.Role,
                Location = "Regional NSW",
                ProfileSummary = user.ProfileSummary,
                Skills = new List<string>(),
                Services = new List<string>(),
                HourlyRate = 0,
                Availability = user.Role == "SME" ? "Seeking delivery partners" : "Available for discovery",
                IsVerified = user.OnboardingCompleted,
                IsDiscoverable = true,
                FeaturedRank = user.Role == "Expert" ? 100 : 70,
                LastActiveAt = user.OnboardedAt ?? DateTime.UtcNow
            };
        }
    }
}
