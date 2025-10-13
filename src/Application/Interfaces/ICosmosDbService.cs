using Microsoft.Azure.Cosmos;

namespace Application.Interfaces;

/// <summary>
/// Interface for Cosmos DB service operations
/// </summary>
public interface ICosmosDbService
{
    CosmosClient Client { get; }
    string DatabaseName { get; }
    Database GetDatabase();
    Container GetContainer(string containerName);
    Task<Database> CreateDatabaseIfNotExistsAsync(int? throughput = null);
    Task<Container> CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath, int? throughput = null);
}
