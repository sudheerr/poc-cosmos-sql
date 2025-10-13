namespace Infrastructure.CosmosDB;

/// <summary>
/// Configuration settings for Cosmos DB
/// </summary>
public class CosmosDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string PartitionKeyPath { get; set; } = "/id";
}
