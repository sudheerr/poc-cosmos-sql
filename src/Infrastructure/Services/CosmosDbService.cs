using Application.Interfaces;
using Infrastructure.CosmosDB;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of Cosmos DB service
/// Registered as Singleton in DI container for connection reuse
/// </summary>
public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;

    public CosmosDbService(CosmosClient client, CosmosDbSettings settings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _databaseName = settings?.DatabaseName ?? throw new ArgumentNullException(nameof(settings));
    }

    public CosmosClient Client => _client;
    public string DatabaseName => _databaseName;

    public Database GetDatabase() => _client.GetDatabase(_databaseName);

    public Container GetContainer(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));

        return _client.GetDatabase(_databaseName).GetContainer(containerName);
    }

    public async Task<Database> CreateDatabaseIfNotExistsAsync(int? throughput = null)
    {
        var response = await _client.CreateDatabaseIfNotExistsAsync(
            _databaseName,
            throughput: throughput);
        return response.Database;
    }

    public async Task<Container> CreateContainerIfNotExistsAsync(
        string containerName,
        string partitionKeyPath,
        int? throughput = null)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));

        if (string.IsNullOrWhiteSpace(partitionKeyPath))
            throw new ArgumentException("Partition key path cannot be null or empty", nameof(partitionKeyPath));

        var database = GetDatabase();
        var response = await database.CreateContainerIfNotExistsAsync(
            containerName,
            partitionKeyPath,
            throughput: throughput);
        return response.Container;
    }
}
