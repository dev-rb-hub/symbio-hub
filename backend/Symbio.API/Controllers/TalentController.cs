using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symbio.Core.Repositories;
using Symbio.Core.Services;

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
        public async Task<IActionResult> Search([FromQuery] string? query = null, [FromQuery] string? skill = null, [FromQuery] string? location = null, [FromQuery] int? minHoursPerWeek = null, [FromQuery] string? postcode = null, [FromQuery] int? radiusKm = null)
        {
            var profiles = await _talentDiscoveryRepository.SearchTalentProfilesAsync(query, skill, location, minHoursPerWeek, postcode, radiusKm);

            GeoPoint? origin = RegionalMatching.TryResolvePostcode(postcode, out var resolved) ? resolved : null;

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
                    profile.Postcode,
                    DistanceKm = ResolveDistanceKm(profile, origin),
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

        internal static double? ResolveDistanceKm(Core.Models.TalentProfile profile, GeoPoint? origin)
        {
            if (origin == null || !profile.TryGetCoordinates(out var point))
            {
                return null;
            }

            return Math.Round(RegionalMatching.DistanceKm(origin.Value, point), 1);
        }
    }
}