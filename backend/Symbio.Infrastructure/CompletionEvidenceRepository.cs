using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Symbio.Core.Models;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure
{
    public class CompletionEvidenceRepository : ICompletionEvidenceRepository
    {
        private static readonly CompletionEvidenceMatrix LocalMatrix = new();
        private static readonly object LocalLock = new();

        private readonly Container? _container;

        public CompletionEvidenceRepository(IConfiguration configuration)
        {
            var connectionString = configuration["Cosmos:ConnectionString"];
            var databaseName = configuration["Cosmos:DatabaseName"] ?? "SymbioHub";
            var containerName = configuration["Cosmos:CompletionEvidenceContainerName"] ?? "CompletionEvidence";

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var client = new CosmosClient(connectionString);
                var database = client.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
                var containerResponse = database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, "/MilestoneId")).GetAwaiter().GetResult();
                _container = containerResponse.Container;
            }
        }

        public async Task RecordAsync(CompletionEvidenceRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            record.LoggedAtUtc = DateTime.UtcNow;

            if (_container == null)
            {
                lock (LocalLock)
                {
                    LocalMatrix.Record(record);
                }

                return;
            }

            await _container.CreateItemAsync(record, new PartitionKey(record.PartitionKey));
        }

        public async Task<IReadOnlyList<CompletionEvidenceRecord>> GetByMilestoneAsync(string milestoneId)
        {
            if (string.IsNullOrWhiteSpace(milestoneId))
            {
                return Array.Empty<CompletionEvidenceRecord>();
            }

            if (_container == null)
            {
                lock (LocalLock)
                {
                    return LocalMatrix.ForMilestone(milestoneId);
                }
            }

            var query = new QueryDefinition("SELECT * FROM c WHERE c.MilestoneId = @milestoneId")
                .WithParameter("@milestoneId", milestoneId);
            var iterator = _container.GetItemQueryIterator<CompletionEvidenceRecord>(query);
            var results = new List<CompletionEvidenceRecord>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.Resource);
            }

            return results
                .OrderByDescending(item => item.LoggedAtUtc)
                .ToList();
        }

        public async Task<IReadOnlyList<CompletionEvidenceRecord>> GetByEpicAsync(string epicId)
        {
            if (string.IsNullOrWhiteSpace(epicId))
            {
                return Array.Empty<CompletionEvidenceRecord>();
            }

            if (_container == null)
            {
                lock (LocalLock)
                {
                    return LocalMatrix.ForEpic(epicId);
                }
            }

            var query = new QueryDefinition("SELECT * FROM c WHERE c.EpicId = @epicId").WithParameter("@epicId", epicId);
            var iterator = _container.GetItemQueryIterator<CompletionEvidenceRecord>(query);
            var results = new List<CompletionEvidenceRecord>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.Resource);
            }

            return results
                .OrderByDescending(item => item.LoggedAtUtc)
                .ToList();
        }

        public async Task<CompletionEvidenceMatrix> GetMatrixAsync()
        {
            if (_container == null)
            {
                lock (LocalLock)
                {
                    var matrix = new CompletionEvidenceMatrix();
                    foreach (var record in LocalMatrix.Records)
                    {
                        matrix.Record(record);
                    }

                    return matrix;
                }
            }

            var iterator = _container.GetItemQueryIterator<CompletionEvidenceRecord>(new QueryDefinition("SELECT * FROM c"));
            var matrixFromStore = new CompletionEvidenceMatrix();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var record in response.Resource)
                {
                    matrixFromStore.Record(record);
                }
            }

            return matrixFromStore;
        }
    }
}