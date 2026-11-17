using System;
using System.Collections.Generic;

namespace Symbio.Core.Services
{
    public readonly record struct GeoPoint(double Latitude, double Longitude);

    /// <summary>
    /// Demo-grade regional matching helpers. The gazetteer is a small, hand-maintained
    /// set of Australian locations for seeded demos and tests, not an authoritative
    /// dataset. A production region pack would supply a licensed source (for example ABS
    /// boundaries) behind the same lookup.
    /// </summary>
    public static class RegionalMatching
    {
        private const double EarthRadiusKm = 6371.0;

        private static readonly IReadOnlyDictionary<string, GeoPoint> Gazetteer = new Dictionary<string, GeoPoint>(StringComparer.OrdinalIgnoreCase)
        {
            ["2000"] = new GeoPoint(-33.8688, 151.2093), // Sydney, NSW
            ["2300"] = new GeoPoint(-32.9283, 151.7817), // Newcastle, NSW
            ["2500"] = new GeoPoint(-34.4278, 150.8931), // Wollongong, NSW
            ["2340"] = new GeoPoint(-31.0905, 150.9291), // Tamworth, NSW
            ["2600"] = new GeoPoint(-35.2820, 149.1286), // Canberra, ACT
            ["3000"] = new GeoPoint(-37.8136, 144.9631), // Melbourne, VIC
            ["4000"] = new GeoPoint(-27.4698, 153.0251), // Brisbane, QLD
        };

        public static bool TryResolvePostcode(string? postcode, out GeoPoint point)
        {
            point = default;
            if (string.IsNullOrWhiteSpace(postcode))
            {
                return false;
            }

            return Gazetteer.TryGetValue(postcode.Trim(), out point);
        }

        public static double DistanceKm(GeoPoint from, GeoPoint to)
        {
            var dLat = ToRadians(to.Latitude - from.Latitude);
            var dLon = ToRadians(to.Longitude - from.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRadians(from.Latitude)) * Math.Cos(ToRadians(to.Latitude))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
    }
}
