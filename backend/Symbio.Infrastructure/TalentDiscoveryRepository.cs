using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Symbio.Core.Models;
using Symbio.Core.Repositories;
using Symbio.Infrastructure.Data;

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

        public async Task<IEnumerable<TalentProfile>> SearchTalentProfilesAsync(string? query = null, string? skill = null, string? location = null, int limit = 12)
        {
            var profiles = await LoadProfilesAsync();

            var filtered = profiles
                .Where(profile => profile.IsDiscoverable)
                .Where(profile => MatchesText(profile, query))
                .Where(profile => MatchesSkill(profile, skill))
                .Where(profile => MatchesLocation(profile, location))
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
                return TalentSeedData.DefaultProfiles.ToList();
            }

            var response = await _container.GetItemQueryIterator<TalentProfile>(new QueryDefinition("SELECT * FROM c WHERE c.IsDiscoverable = true")).ReadNextAsync();
            var profiles = response.Resource.ToList();

            if (!profiles.Any())
            {
                foreach (var profile in TalentSeedData.DefaultProfiles)
                {
                    await _container.UpsertItemAsync(profile, new PartitionKey(profile.Role));
                }

                var seededResponse = await _container.GetItemQueryIterator<TalentProfile>(new QueryDefinition("SELECT * FROM c WHERE c.IsDiscoverable = true")).ReadNextAsync();
                profiles = seededResponse.Resource.ToList();
            }

            return profiles;
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