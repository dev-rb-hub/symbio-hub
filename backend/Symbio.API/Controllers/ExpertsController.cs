using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symbio.Core.Repositories;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/experts")]
    public class ExpertsController : ControllerBase
    {
        private readonly ITalentDiscoveryRepository _talentDiscoveryRepository;

        public ExpertsController(ITalentDiscoveryRepository talentDiscoveryRepository)
        {
            _talentDiscoveryRepository = talentDiscoveryRepository;
        }

        [HttpGet("search")]
        [Authorize(Roles = "SME")]
        public async Task<IActionResult> Search(
            [FromQuery] string? query = null,
            [FromQuery] string? skill = null,
            [FromQuery] string? location = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var profiles = (await _talentDiscoveryRepository.SearchTalentProfilesAsync(query, skill, location, 200)).ToList();
            var paged = profiles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                page,
                pageSize,
                total = profiles.Count,
                results = paged.Select(profile => new
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
                })
            });
        }
    }
}