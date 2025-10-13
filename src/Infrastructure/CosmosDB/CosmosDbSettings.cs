namespace Infrastructure.CosmosDB;

public class CosmosDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string AccountEndpoint { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public int Throughput { get; set; } = 400;
}

public class CosmosDbConfiguration
{
    public CosmosDbSettings ProductsDatabase { get; set; } = new();
    public CosmosDbSettings OrdersDatabase { get; set; } = new();
    public CosmosDbSettings CustomersDatabase { get; set; } = new();
}
