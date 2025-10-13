using Application.Interfaces;
using Domain.Entities;
using Infrastructure.CosmosDB;
using Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for setting up services in dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server infrastructure services
    /// </summary>
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "SqlServerConnection")
    {
        // Register SQL Server DbContext
        services.AddDbContext<SqlServerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(connectionStringName),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                )
            )
        );

        // Register repositories for SQL Server
        services.AddScoped<IDataRepository<Product>, SqlServerRepository<Product>>();
        services.AddScoped<IDataRepository<Customer>, SqlServerRepository<Customer>>();
        services.AddScoped<IDataRepository<Order>, SqlServerRepository<Order>>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, SqlServerUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds multiple SQL Server database contexts for different databases
    /// </summary>
    public static IServiceCollection AddMultipleSqlServerDatabases(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Example: Register multiple DbContexts for different databases
        services.AddDbContext<SqlServerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

        // You can add more contexts here for different databases
        // services.AddDbContext<OrdersDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection_OrdersDb")));

        return services;
    }

    /// <summary>
    /// Adds Cosmos DB infrastructure services with singleton pattern
    /// </summary>
    public static IServiceCollection AddCosmosDbInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Cosmos DB settings
        var cosmosDbSettings = new CosmosDbSettings
        {
            ConnectionString = configuration["CosmosDb:ConnectionString"]
                ?? throw new InvalidOperationException("CosmosDb connection string not found"),
            DatabaseName = configuration["CosmosDb:DatabaseName"] ?? "ProductsDb",
            ContainerName = configuration["CosmosDb:ContainerName"] ?? "Products",
            PartitionKeyPath = configuration["CosmosDb:PartitionKeyPath"] ?? "/id"
        };

        // Register Cosmos DB service as singleton
        services.AddSingleton(cosmosDbSettings);
        services.AddSingleton<CosmosDbService>();

        // Register Cosmos DB repositories
        // Uncomment these to use Cosmos DB for specific entities
        // services.AddScoped<IDataRepository<Product>, CosmosDbRepository<Product>>();
        // services.AddScoped<IDataRepository<Customer>, CosmosDbRepository<Customer>>();
        // services.AddScoped<IDataRepository<Order>, CosmosDbRepository<Order>>();

        return services;
    }

    /// <summary>
    /// Adds infrastructure services with both SQL Server and Cosmos DB
    /// This method allows you to configure which entities use which database
    /// </summary>
    public static IServiceCollection AddHybridInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // SQL Server setup
        services.AddDbContext<SqlServerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServerConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()
            )
        );

        // Cosmos DB setup (singleton)
        var cosmosDbSettings = new CosmosDbSettings
        {
            ConnectionString = configuration["CosmosDb:ConnectionString"]
                ?? throw new InvalidOperationException("CosmosDb connection string not found"),
            DatabaseName = configuration["CosmosDb:DatabaseName"] ?? "ProductsDb",
            ContainerName = configuration["CosmosDb:ContainerName"] ?? "Products",
            PartitionKeyPath = configuration["CosmosDb:PartitionKeyPath"] ?? "/id"
        };

        services.AddSingleton(cosmosDbSettings);
        services.AddSingleton<CosmosDbService>();

        // Configure which entities use which database
        // Products -> Cosmos DB (for high-scale catalog)
        services.AddScoped<IDataRepository<Product>, CosmosDbRepository<Product>>();

        // Customers and Orders -> SQL Server (for transactional data)
        services.AddScoped<IDataRepository<Customer>, SqlServerRepository<Customer>>();
        services.AddScoped<IDataRepository<Order>, SqlServerRepository<Order>>();

        // Register Unit of Work for SQL Server transactions
        services.AddScoped<IUnitOfWork, SqlServerUnitOfWork>();

        return services;
    }
}
