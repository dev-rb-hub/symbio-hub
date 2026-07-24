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
    [Route("api/payments/onboarding")]
    [Authorize(Roles = "Expert")]
    public class EscrowOnboardingController : ControllerBase
    {
        private readonly SymbioDbContext _dbContext;
        private readonly IPinchOnboardingGateway _pinchOnboardingGateway;

        public EscrowOnboardingController(SymbioDbContext dbContext, IPinchOnboardingGateway pinchOnboardingGateway)
        {
            _dbContext = dbContext;
            _pinchOnboardingGateway = pinchOnboardingGateway;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var profile = await _dbContext.EscrowOnboardingProfiles
                .FirstOrDefaultAsync(item => item.ExpertEmail == email);

            if (profile == null)
            {
                return Ok(new
                {
                    expertEmail = email,
                    status = EscrowOnboardingStatus.NotStarted.ToString(),
                    providerAccountId = string.Empty,
                    onboardingUrl = string.Empty,
                    lastSyncedAtUtc = (DateTime?)null,
                    onboardedAtUtc = (DateTime?)null
                });
            }

            return Ok(new
            {
                expertEmail = profile.ExpertEmail,
                status = profile.Status,
                providerAccountId = profile.ProviderAccountId,
                onboardingUrl = profile.OnboardingUrl,
                profile.LastSyncedAtUtc,
                profile.OnboardedAtUtc
            });
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var session = await _pinchOnboardingGateway.CreateExpertOnboardingSessionAsync(email);
            var profile = await _dbContext.EscrowOnboardingProfiles
                .FirstOrDefaultAsync(item => item.ExpertEmail == email);

            if (profile == null)
            {
                profile = new EscrowOnboardingProfile
                {
                    ExpertEmail = email,
                    ProviderAccountId = session.ProviderAccountId,
                    Status = session.Status.ToString(),
                    OnboardingUrl = session.OnboardingUrl,
                    LastSyncedAtUtc = DateTime.UtcNow
                };

                _dbContext.EscrowOnboardingProfiles.Add(profile);
            }
            else
            {
                profile.ProviderAccountId = session.ProviderAccountId;
                profile.Status = session.Status.ToString();
                profile.OnboardingUrl = session.OnboardingUrl;
                profile.LastSyncedAtUtc = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                profile.ExpertEmail,
                profile.Status,
                profile.ProviderAccountId,
                profile.OnboardingUrl,
                profile.LastSyncedAtUtc,
                profile.OnboardedAtUtc
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var profile = await _dbContext.EscrowOnboardingProfiles
                .FirstOrDefaultAsync(item => item.ExpertEmail == email);

            if (profile == null)
            {
                return NotFound(new { message = "Escrow onboarding has not started." });
            }

            var status = await _pinchOnboardingGateway.GetOnboardingStatusAsync(profile.ProviderAccountId);
            profile.Status = status.ToString();
            profile.LastSyncedAtUtc = DateTime.UtcNow;

            if (status == EscrowOnboardingStatus.Verified && profile.OnboardedAtUtc == null)
            {
                profile.OnboardedAtUtc = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                profile.ExpertEmail,
                profile.Status,
                profile.ProviderAccountId,
                profile.OnboardingUrl,
                profile.LastSyncedAtUtc,
                profile.OnboardedAtUtc
            });
        }

        [HttpPost("simulate-complete")]
        public async Task<IActionResult> SimulateComplete()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var profile = await _dbContext.EscrowOnboardingProfiles
                .FirstOrDefaultAsync(item => item.ExpertEmail == email);

            if (profile == null)
            {
                return NotFound(new { message = "Escrow onboarding has not started." });
            }

            if (!profile.ProviderAccountId.Contains("verified", StringComparison.OrdinalIgnoreCase))
            {
                profile.ProviderAccountId = $"{profile.ProviderAccountId}-verified";
            }

            profile.Status = EscrowOnboardingStatus.Verified.ToString();
            profile.LastSyncedAtUtc = DateTime.UtcNow;
            profile.OnboardedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                profile.ExpertEmail,
                profile.Status,
                profile.ProviderAccountId,
                profile.OnboardingUrl,
                profile.LastSyncedAtUtc,
                profile.OnboardedAtUtc
            });
        }
    }
}