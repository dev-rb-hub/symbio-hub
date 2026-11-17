using System;
using System.Collections.Generic;

namespace Symbio.Core.Models
{
    public class TalentProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Expert";
        public string Location { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string ProfileSummary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<string> Services { get; set; } = new();
        public decimal HourlyRate { get; set; }
        public string Availability { get; set; } = string.Empty;
        public int? HoursPerWeek { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public bool IsVerified { get; set; } = true;
        public bool IsDiscoverable { get; set; } = true;
        public int FeaturedRank { get; set; }
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

        public string ResolveAvailabilityStatus(DateTime utcNow)
        {
            if (!HoursPerWeek.HasValue)
            {
                return "unknown";
            }

            if (AvailableUntil.HasValue && AvailableUntil.Value.Date < utcNow.Date)
            {
                return "expired";
            }

            return "available";
        }

        public bool TryGetCoordinates(out Symbio.Core.Services.GeoPoint point)
        {
            if (Latitude.HasValue && Longitude.HasValue)
            {
                point = new Symbio.Core.Services.GeoPoint(Latitude.Value, Longitude.Value);
                return true;
            }

            return Symbio.Core.Services.RegionalMatching.TryResolvePostcode(Postcode, out point);
        }
    }
}