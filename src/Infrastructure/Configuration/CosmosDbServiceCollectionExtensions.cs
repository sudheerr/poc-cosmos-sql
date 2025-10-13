using Application.Interfaces;
using Domain.Entities;
using Infrastructure.CosmosDB;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

public static class CosmosDbServiceCollectionExtensions
{
    /// <summary>
    /// Add single Cosmos DB database configuration
    /// </summary>
    public static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "CosmosDb")
    {
        var settings = configuration.GetSection(configSection).Get<CosmosDbSettings>()
            ?? throw new InvalidOperationException($"CosmosDb configuration section '{configSection}' not found");

        // Register singleton CosmosClient
        services.AddSingleton(sp =>
        {
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 3,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                RequestTimeout = TimeSpan.FromSeconds(60)
            };

            return new CosmosClient(settings.ConnectionString, cosmosClientOptions);
        });

        // Register settings
        services.AddSingleton(settings);

        // Register Cosmos DB service as singleton
        services.AddSingleton<ICosmosDbService, CosmosDbService>();

        // Register repositories as scoped
        services.AddScoped<IRepository<Product>, ProductRepository>();
        services.AddScoped<IRepository<Customer>, CustomerRepository>();
        services.AddScoped<IRepository<Order>, OrderRepository>();

        // Register specific repositories
        services.AddScoped<ProductRepository>();
        services.AddScoped<CustomerRepository>();
        services.AddScoped<OrderRepository>();

        return services;
    }

    /// <summary>
    /// Add multiple Cosmos DB databases configuration
    /// Each database gets its own service and repositories
    /// </summary>
    public static IServiceCollection AddMultipleCosmosDbDatabases(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var productsConfig = configuration.GetSection("CosmosDb:ProductsDatabase").Get<CosmosDbSettings>();
        var customersConfig = configuration.GetSection("CosmosDb:CustomersDatabase").Get<CosmosDbSettings>();
        var ordersConfig = configuration.GetSection("CosmosDb:OrdersDatabase").Get<CosmosDbSettings>();

        if (productsConfig != null)
        {
            services.AddCosmosDbForEntity<Product>(productsConfig, "Products");
        }

        if (customersConfig != null)
        {
            services.AddCosmosDbForEntity<Customer>(customersConfig, "Customers");
        }

        if (ordersConfig != null)
        {
            services.AddCosmosDbForEntity<Order>(ordersConfig, "Orders");
        }

        return services;
    }

    /// <summary>
    /// Add Cosmos DB configuration for a specific entity with its own database
    /// </summary>
    private static IServiceCollection AddCosmosDbForEntity<TEntity>(
        this IServiceCollection services,
        CosmosDbSettings settings,
        string serviceName) where TEntity : BaseEntity
    {
        // Register named singleton CosmosClient
        services.AddSingleton(sp =>
        {
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 3,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
            };

            var client = new CosmosClient(settings.ConnectionString, cosmosClientOptions);

            // Create database and container if they don't exist
            var database = client.CreateDatabaseIfNotExistsAsync(
                settings.DatabaseName,
                throughput: settings.Throughput).GetAwaiter().GetResult();

            database.Database.CreateContainerIfNotExistsAsync(
                serviceName,
                $"/{GetPartitionKeyPath<TEntity>()}",
                throughput: settings.Throughput).GetAwaiter().GetResult();

            return new CosmosDbService(client, settings);
        });

        return services;
    }

    /// <summary>
    /// Get partition key path for entity type
    /// </summary>
    private static string GetPartitionKeyPath<TEntity>() where TEntity : BaseEntity
    {
        return typeof(TEntity).Name switch
        {
            nameof(Product) => "category",
            nameof(Customer) => "country",
            nameof(Order) => "customerId",
            _ => "id"
        };
    }

    /// <summary>
    /// Initialize Cosmos DB databases and containers
    /// Call this during application startup
    /// </summary>
    public static async Task InitializeCosmosDbAsync(
        this IServiceProvider serviceProvider,
        params (string containerName, string partitionKeyPath)[] containers)
    {
        var cosmosDbService = serviceProvider.GetRequiredService<ICosmosDbService>();

        // Create database
        await cosmosDbService.CreateDatabaseIfNotExistsAsync();

        // Create containers
        foreach (var (containerName, partitionKeyPath) in containers)
        {
            await cosmosDbService.CreateContainerIfNotExistsAsync(
                containerName,
                partitionKeyPath);
        }
    }
}
