# Quick Start Guide

## Prerequisites
- .NET 8 SDK installed
- SQL Server (LocalDB, Express, or Full) OR Cosmos DB Emulator
- Terminal/Command Prompt

## Step 1: Create the Solution Structure

Choose your platform and run the appropriate script:

### Windows (PowerShell)
```powershell
.\create-solution.ps1
```

### Windows (Command Prompt)
```cmd
create-solution.bat
```

### Linux/macOS
```bash
chmod +x create-solution.sh
./create-solution.sh
```

### Manual Creation (if scripts don't work)
```bash
# See SETUP-GUIDE.md for manual step-by-step instructions
```

## Step 2: Verify Solution Structure

After running the script, you should have:

```
poc-cosmos-sql/
├── CosmosDbSqlApi.sln          ← Solution file
├── src/
│   ├── Domain/
│   │   └── Domain.csproj
│   ├── Application/
│   │   └── Application.csproj
│   ├── Infrastructure/
│   │   └── Infrastructure.csproj
│   └── API/
│       └── API.csproj
└── tests/
    ├── Domain.Tests/
    ├── API.Tests/
    └── Infrastructure.Tests/
```

## Step 3: Copy Source Files

All source files have been created in the repository. The files are already in place:

- ✅ Domain entities (Product, Customer, Order)
- ✅ Application interfaces (IDataRepository, IUnitOfWork)
- ✅ Infrastructure implementations (SqlServer, CosmosDB)
- ✅ API controllers and Program.cs
- ✅ Test files with Moq

Just make sure they're in the correct project directories!

## Step 4: Build the Solution

```bash
dotnet build
```

If you see errors about missing files, ensure all the generated `.cs` files are in their correct locations.

## Step 5: Update Configuration

Edit `src/API/appsettings.json`:

### For Local Development with SQL Server LocalDB:
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "ProductsDb",
    "ContainerName": "Products"
  }
}
```

## Step 6: Create Database

### For SQL Server:
```bash
cd src/API
dotnet ef migrations add InitialCreate --project ../Infrastructure
dotnet ef database update --project ../Infrastructure
```

### For Cosmos DB Emulator:
1. Download and install [Azure Cosmos DB Emulator](https://aka.ms/cosmosdb-emulator)
2. Start the emulator
3. The database and container will be created automatically on first run

## Step 7: Run the Application

```bash
cd src/API
dotnet run
```

The API will start at:
- HTTPS: https://localhost:7xxx
- HTTP: http://localhost:5xxx
- Swagger: https://localhost:7xxx/swagger

## Step 8: Test the API

### Using Swagger UI
1. Open browser to https://localhost:7xxx/swagger
2. Try the POST /api/products endpoint to create a product
3. Try the GET /api/products endpoint to retrieve products

### Using curl
```bash
# Create a product
curl -X POST https://localhost:7001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "category": "Electronics"
  }'

# Get all products
curl https://localhost:7001/api/products
```

## Step 9: Run Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/API.Tests
dotnet test tests/Infrastructure.Tests
dotnet test tests/Domain.Tests

# Run with detailed output
dotnet test --verbosity detailed
```

## Troubleshooting

### Issue: "dotnet command not found"
**Solution:** Install .NET 8 SDK from https://dotnet.microsoft.com/download

### Issue: "Cannot create database"
**Solution:**
- For LocalDB: Install SQL Server Express with LocalDB
- Or update connection string to use your SQL Server instance

### Issue: "EF Core tools not found"
**Solution:**
```bash
dotnet tool install --global dotnet-ef
```

### Issue: Solution file not found
**Solution:** Run the appropriate create-solution script for your platform

### Issue: Build errors about missing namespaces
**Solution:** Make sure all `.cs` files are in the correct project directories:
- Domain entities → `src/Domain/Entities/`
- Application interfaces → `src/Application/Interfaces/`
- Infrastructure → `src/Infrastructure/SqlServer/` and `src/Infrastructure/CosmosDB/`
- API → `src/API/Controllers/`
- Tests → `tests/{ProjectName}/`

## Next Steps

1. ✅ Solution is running
2. 📖 Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand the design
3. 🧪 Review [TESTING-GUIDE.md](TESTING-GUIDE.md) for testing information
4. 🔧 Check [SETUP-GUIDE.md](SETUP-GUIDE.md) for advanced configuration
5. 🚀 Start building your features!

## Project Structure Overview

```
poc-cosmos-sql/
├── CosmosDbSqlApi.sln
├── src/
│   ├── Domain/                     # Entities (Product, Customer, Order)
│   ├── Application/                # Interfaces (IDataRepository<T>)
│   ├── Infrastructure/             # Data Access
│   │   ├── SqlServer/             # EF Core implementation
│   │   ├── CosmosDB/              # Cosmos DB implementation (singleton)
│   │   └── DependencyInjection/   # Service registration
│   └── API/                       # Controllers and configuration
└── tests/
    ├── Domain.Tests/              # Entity tests
    ├── API.Tests/                 # Controller tests (Moq) + Integration tests
    └── Infrastructure.Tests/      # Repository tests (InMemory DB)
```

## Key Features

✅ Clean Architecture
✅ Generic Repository Pattern (`IDataRepository<T>`)
✅ Cosmos DB with Singleton Pattern
✅ SQL Server with Entity Framework Core
✅ LINQ Support for both databases
✅ Unit of Work for transactions
✅ Comprehensive tests with Moq
✅ Swagger documentation
✅ Dependency Injection

## Support

- 📚 Documentation: See README.md, ARCHITECTURE.md, TESTING-GUIDE.md
- 🐛 Issues: Check the error messages and troubleshooting section
- 💡 Examples: Look at the test files for usage examples
