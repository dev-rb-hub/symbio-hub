using Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly Container? _container;

        public ProjectRepository(IConfiguration configuration)
        {
            var connectionString = configuration["Cosmos:ConnectionString"];
            var databaseName = configuration["Cosmos:DatabaseName"] ?? "SymbioHub";
            var containerName = configuration["Cosmos:ContainerName"] ?? "Projects";

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var client = new CosmosClient(connectionString);
                var database = client.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
                var containerResponse = database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, "/Category")).GetAwaiter().GetResult();
                _container = containerResponse.Container;
            }
        }

        public async Task<ProjectScope> SaveProjectAsync(ProjectScope project)
        {
            if (_container != null)
            {
                await _container.CreateItemAsync(project, new PartitionKey(project.Category));
            }

            return project;
        }

        public async Task<IEnumerable<ProjectScope>> GetPublishedProjectsAsync()
        {
            if (_container == null)
            {
                return Array.Empty<ProjectScope>();
            }

            var query = new QueryDefinition("SELECT * FROM c WHERE c.IsPublished = @published").WithParameter("@published", true);
            var iterator = _container.GetItemQueryIterator<ProjectScope>(query);
            var results = new List<ProjectScope>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.Resource);
            }

            return results;
        }
    }
}
