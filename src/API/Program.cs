using Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Cosmos DB - Single database approach
builder.Services.AddCosmosDb(builder.Configuration);

// OR use multiple databases (uncomment to use)
// builder.Services.AddMultipleCosmosDbDatabases(builder.Configuration);

var app = builder.Build();

// Initialize Cosmos DB (create database and containers)
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.InitializeCosmosDbAsync(
        ("Products", "/category"),
        ("Customers", "/country"),
        ("Orders", "/customerId")
    );
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
