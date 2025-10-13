using Microsoft.Azure.Cosmos;

namespace Infrastructure.CosmosDB;

/// <summary>
/// Singleton Cosmos DB service for managing the client instance
/// This ensures a single Cosmos DB client is shared across the application
/// </summary>
public class CosmosDbService
{
    private static CosmosClient? _cosmosClient;
    private static readonly object _lock = new object();
    private readonly CosmosDbSettings _settings;

    public CosmosDbService(CosmosDbSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Gets the singleton Cosmos DB client instance
    /// </summary>
    public CosmosClient GetClient()
    {
        if (_cosmosClient == null)
        {
            lock (_lock)
            {
                if (_cosmosClient == null)
                {
                    var options = new CosmosClientOptions
                    {
                        SerializerOptions = new CosmosSerializationOptions
                        {
                            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                        },
                        ConnectionMode = ConnectionMode.Direct,
                        MaxRetryAttemptsOnRateLimitedRequests = 3,
                        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
                    };

                    _cosmosClient = new CosmosClient(_settings.ConnectionString, options);
                }
            }
        }

        return _cosmosClient;
    }

    /// <summary>
    /// Gets or creates the database and container
    /// </summary>
    public async Task<Container> GetContainerAsync()
    {
        var client = GetClient();

        // Create database if it doesn't exist
        var database = await client.CreateDatabaseIfNotExistsAsync(_settings.DatabaseName);

        // Create container if it doesn't exist
        var containerResponse = await database.Database.CreateContainerIfNotExistsAsync(
            _settings.ContainerName,
            _settings.PartitionKeyPath,
            throughput: 400 // Adjust based on your needs
        );

        return containerResponse.Container;
    }

    /// <summary>
    /// Disposes the Cosmos DB client
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _cosmosClient?.Dispose();
            _cosmosClient = null;
        }
    }
}
