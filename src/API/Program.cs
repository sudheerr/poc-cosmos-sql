using Application.Interfaces;
using Domain.Entities;
using Infrastructure.CosmosDB;
using Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== SQL Server Configuration =====
// Register SQL Server DbContext for multiple databases if needed
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServerConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// Register SQL Server repositories
builder.Services.AddScoped<IDataRepository<Product>, SqlServerRepository<Product>>();
builder.Services.AddScoped<IDataRepository<Customer>, SqlServerRepository<Customer>>();
builder.Services.AddScoped<IDataRepository<Order>, SqlServerRepository<Order>>();

// Register Unit of Work for SQL Server
builder.Services.AddScoped<IUnitOfWork, SqlServerUnitOfWork>();

// ===== Cosmos DB Configuration (Singleton Pattern) =====
// Register Cosmos DB settings
var cosmosDbSettings = new CosmosDbSettings
{
    ConnectionString = builder.Configuration["CosmosDb:ConnectionString"] ?? throw new InvalidOperationException("CosmosDb connection string not found"),
    DatabaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "ProductsDb",
    ContainerName = builder.Configuration["CosmosDb:ContainerName"] ?? "Products",
    PartitionKeyPath = builder.Configuration["CosmosDb:PartitionKeyPath"] ?? "/id"
};

// Register Cosmos DB service as singleton
builder.Services.AddSingleton(cosmosDbSettings);
builder.Services.AddSingleton<CosmosDbService>();

// Register Cosmos DB repositories (if you want to use Cosmos DB for specific entities)
// Uncomment these if you want to use Cosmos DB instead of SQL Server for these entities
// builder.Services.AddScoped<IDataRepository<Product>, CosmosDbRepository<Product>>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SqlServerDbContext>();

    // Apply migrations automatically (optional - comment out if you prefer manual migrations)
    // await dbContext.Database.MigrateAsync();

    // Ensure database is created (for development)
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
