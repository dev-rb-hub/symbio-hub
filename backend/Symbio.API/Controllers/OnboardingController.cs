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
        private readonly IIdentityVerificationService _identityVerificationService;
        private readonly IPinchMerchantService _pinchMerchantService;

        public OnboardingController(
            SymbioDbContext dbContext,
            ITalentDiscoveryRepository talentDiscoveryRepository,
            IIdentityVerificationService identityVerificationService,
            IPinchMerchantService pinchMerchantService)
        {
            _dbContext = dbContext;
            _talentDiscoveryRepository = talentDiscoveryRepository;
            _identityVerificationService = identityVerificationService;
            _pinchMerchantService = pinchMerchantService;
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

            var identifierValid = await _identityVerificationService.ValidateBusinessIdentifierAsync(request.BusinessIdentifier ?? string.Empty);
            if (!identifierValid)
            {
                return BadRequest(new { message = "Business identifier failed validation." });
            }

            user.CompanyName = request.CompanyName ?? string.Empty;
            user.BusinessIdentifier = request.BusinessIdentifier ?? string.Empty;
            user.ProfileSummary = request.ProfileSummary ?? string.Empty;
            user.OnboardingCompleted = true;
            user.OnboardedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            if (string.Equals(user.Role, "Expert", StringComparison.OrdinalIgnoreCase))
            {
                await _talentDiscoveryRepository.UpsertTalentProfileAsync(MapToTalentProfile(user));

                var registration = await _pinchMerchantService.RegisterSubMerchantAsync(
                    user.Email,
                    user.BusinessIdentifier,
                    user.CompanyName);

                var onboardingProfile = await _dbContext.EscrowOnboardingProfiles
                    .FirstOrDefaultAsync(item => item.ExpertEmail == user.Email);

                if (onboardingProfile == null)
                {
                    onboardingProfile = new EscrowOnboardingProfile
                    {
                        ExpertEmail = user.Email,
                        ProviderAccountId = registration.MerchantId,
                        Status = EscrowOnboardingStatus.Pending.ToString(),
                        OnboardingUrl = registration.OnboardingUrl,
                        LastSyncedAtUtc = DateTime.UtcNow
                    };
                    _dbContext.EscrowOnboardingProfiles.Add(onboardingProfile);
                }
                else
                {
                    onboardingProfile.ProviderAccountId = registration.MerchantId;
                    onboardingProfile.Status = EscrowOnboardingStatus.Pending.ToString();
                    onboardingProfile.OnboardingUrl = registration.OnboardingUrl;
                    onboardingProfile.LastSyncedAtUtc = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
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
