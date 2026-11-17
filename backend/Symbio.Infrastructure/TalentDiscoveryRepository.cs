using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Symbio.Core.Services;

namespace Symbio.Infrastructure
{
    public class TalentDiscoveryRepository : ITalentDiscoveryRepository
    {
        private readonly Container? _container;

        public TalentDiscoveryRepository(IConfiguration configuration)
        {
            var connectionString = configuration["Cosmos:ConnectionString"];
            var databaseName = configuration["Cosmos:DatabaseName"] ?? "SymbioHub";
            var containerName = configuration["Cosmos:TalentContainerName"] ?? "TalentProfiles";

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var client = new CosmosClient(connectionString);
                var database = client.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
                var containerResponse = database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, "/Role")).GetAwaiter().GetResult();
                _container = containerResponse.Container;
            }
        }

        public async Task<IEnumerable<TalentProfile>> SearchTalentProfilesAsync(string? query = null, string? skill = null, string? location = null, int? minHoursPerWeek = null, string? postcode = null, int? radiusKm = null, int limit = 12)
        {
            var profiles = await LoadProfilesAsync();

            var radiusActive = !string.IsNullOrWhiteSpace(postcode) && radiusKm.HasValue && radiusKm.Value > 0;
            GeoPoint? origin = null;
            if (radiusActive && RegionalMatching.TryResolvePostcode(postcode, out var resolved))
            {
                origin = resolved;
            }

            var filtered = profiles
                .Where(profile => profile.IsDiscoverable)
                .Where(profile => string.Equals(profile.Role, "Expert", StringComparison.OrdinalIgnoreCase))
                .Where(profile => profile.IsVerified)
                .Where(profile => MatchesText(profile, query))
                .Where(profile => MatchesSkill(profile, skill))
                .Where(profile => MatchesLocation(profile, location))
                .Where(profile => MatchesMinHours(profile, minHoursPerWeek))
                .Where(profile => MatchesRadius(profile, radiusActive, origin, radiusKm))
                .OrderByDescending(profile => profile.FeaturedRank)
                .ThenByDescending(profile => profile.LastActiveAt)
                .Take(limit)
                .ToList();

            return filtered;
        }

        public async Task UpsertTalentProfileAsync(TalentProfile profile)
        {
            if (_container == null)
            {
                return;
            }

            profile.LastActiveAt = DateTime.UtcNow;
            profile.IsDiscoverable = true;

            await _container.UpsertItemAsync(profile, new PartitionKey(profile.Role));
        }

        private async Task<List<TalentProfile>> LoadProfilesAsync()
        {
            if (_container == null)
            {
                return TalentSeedProfiles.DefaultProfiles
                    .Select(CloneProfile)
                    .ToList();
            }

            var response = await _container.GetItemQueryIterator<TalentProfile>(new QueryDefinition("SELECT * FROM c WHERE c.IsDiscoverable = true")).ReadNextAsync();
            var profiles = response.Resource.ToList();

            if (!profiles.Any())
            {
                return TalentSeedProfiles.DefaultProfiles
                    .Select(CloneProfile)
                    .ToList();
            }

            return profiles;
        }

        private static TalentProfile CloneProfile(TalentProfile profile)
        {
            return new TalentProfile
            {
                Id = profile.Id,
                Name = profile.Name,
                CompanyName = profile.CompanyName,
                Email = profile.Email,
                Role = profile.Role,
                Location = profile.Location,
                Postcode = profile.Postcode,
                Latitude = profile.Latitude,
                Longitude = profile.Longitude,
                ProfileSummary = profile.ProfileSummary,
                Skills = profile.Skills.ToList(),
                Services = profile.Services.ToList(),
                HourlyRate = profile.HourlyRate,
                Availability = profile.Availability,
                HoursPerWeek = profile.HoursPerWeek,
                AvailableUntil = profile.AvailableUntil,
                IsVerified = profile.IsVerified,
                IsDiscoverable = profile.IsDiscoverable,
                FeaturedRank = profile.FeaturedRank,
                LastActiveAt = profile.LastActiveAt
            };
        }

        private static bool MatchesText(TalentProfile profile, string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return ContainsIgnoreCase(profile.Name, query)
                || ContainsIgnoreCase(profile.CompanyName, query)
                || ContainsIgnoreCase(profile.Location, query)
                || ContainsIgnoreCase(profile.ProfileSummary, query)
                || profile.Skills.Any(skill => ContainsIgnoreCase(skill, query))
                || profile.Services.Any(service => ContainsIgnoreCase(service, query));
        }

        private static bool MatchesSkill(TalentProfile profile, string? skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
            {
                return true;
            }

            return profile.Skills.Any(candidate => string.Equals(candidate, skill, StringComparison.OrdinalIgnoreCase));
        }

        private static bool MatchesLocation(TalentProfile profile, string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return true;
            }

            return ContainsIgnoreCase(profile.Location, location);
        }

        private static bool MatchesMinHours(TalentProfile profile, int? minHoursPerWeek)
        {
            if (!minHoursPerWeek.HasValue || minHoursPerWeek.Value <= 0)
            {
                return true;
            }

            if (!string.Equals(profile.ResolveAvailabilityStatus(DateTime.UtcNow), "available", StringComparison.Ordinal))
            {
                return false;
            }

            return profile.HoursPerWeek.GetValueOrDefault() >= minHoursPerWeek.Value;
        }

        private static bool MatchesRadius(TalentProfile profile, bool radiusActive, GeoPoint? origin, int? radiusKm)
        {
            if (!radiusActive)
            {
                return true;
            }

            if (origin == null || !radiusKm.HasValue)
            {
                return false;
            }

            if (!profile.TryGetCoordinates(out var profilePoint))
            {
                return false;
            }

            return RegionalMatching.DistanceKm(origin.Value, profilePoint) <= radiusKm.Value;
        }

        private static bool ContainsIgnoreCase(string? source, string? value)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return source.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}