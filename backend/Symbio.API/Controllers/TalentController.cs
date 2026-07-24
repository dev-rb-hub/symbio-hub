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
        public async Task<IActionResult> Search([FromQuery] string? query = null, [FromQuery] string? skill = null, [FromQuery] string? location = null)
        {
            var profiles = await _talentDiscoveryRepository.SearchTalentProfilesAsync(query, skill, location);

            var response = profiles.Select(profile => new
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
                profile.IsVerified,
                profile.FeaturedRank,
                profile.LastActiveAt
            });

            return Ok(response);
        }
    }
}