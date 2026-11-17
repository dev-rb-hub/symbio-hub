using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TalentController : ControllerBase
    {
        private readonly ITalentDiscoveryRepository _talentDiscoveryRepository;

        public TalentController(ITalentDiscoveryRepository talentDiscoveryRepository)
        {
            _talentDiscoveryRepository = talentDiscoveryRepository;
        }

        [HttpGet]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> Search([FromQuery] string? query = null, [FromQuery] string? skill = null, [FromQuery] string? location = null, [FromQuery] int? minHoursPerWeek = null)
        {
            var profiles = await _talentDiscoveryRepository.SearchTalentProfilesAsync(query, skill, location, minHoursPerWeek);

            var now = DateTime.UtcNow;
            var response = profiles.Select(profile =>
            {
                var availabilityStatus = profile.ResolveAvailabilityStatus(now);
                return new
                {
                    profile.Id,
                    profile.Name,
                    profile.CompanyName,
                    profile.Location,
                    profile.ProfileSummary,
                    profile.Skills,
                    profile.Services,
                    profile.HourlyRate,
                    profile.Availability,
                    HoursPerWeek = availabilityStatus == "available" ? profile.HoursPerWeek : null,
                    AvailableUntil = profile.AvailableUntil,
                    AvailabilityStatus = availabilityStatus,
                    profile.IsVerified,
                    profile.FeaturedRank,
                    profile.LastActiveAt
                };
            });

            return Ok(response);
        }
    }
}