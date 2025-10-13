# Getting Started - Clean Architecture .NET 8

## ✅ Solution Created!

Your Clean Architecture solution with .NET 8 is ready!

## 📁 Project Structure

```
CleanArchitecture.sln                    ← Solution file
├── src/
│   ├── Domain/                          ← Core business layer
│   │   ├── Domain.csproj
│   │   └── Entities/
│   │       └── BaseEntity.cs            ← Base entity class
│   │
│   ├── Application/                     ← Application business rules
│   │   ├── Application.csproj
│   │   └── Interfaces/
│   │       └── IRepository.cs           ← Generic repository interface
│   │
│   ├── Infrastructure/                  ← Data access & external services
│   │   └── Infrastructure.csproj
│   │
│   └── API/                            ← Web API layer
│       ├── API.csproj
│       ├── Program.cs                   ← Application entry point
│       └── appsettings.json
│
├── README.md                            ← Architecture documentation
└── GETTING-STARTED.md                   ← This file
```

## 🚀 Quick Start

### 1. Build the Solution
```bash
dotnet build CleanArchitecture.sln
```

### 2. Run the API
```bash
cd src/API
dotnet run
```

### 3. Access Swagger UI
Open your browser to:
- **Swagger**: https://localhost:7xxx/swagger
- **API**: https://localhost:7xxx

## 📦 What's Included

### Domain Layer
- ✅ **BaseEntity.cs** - Base class for all entities with:
  - `Id` (string, auto-generated GUID)
  - `CreatedAt` (DateTime)
  - `UpdatedAt` (DateTime?)
  - `IsDeleted` (soft delete flag)

### Application Layer
- ✅ **IRepository<T>** - Generic repository interface with:
  - `GetByIdAsync()`
  - `GetAllAsync()`
  - `FindAsync()`
  - `AddAsync()`
  - `UpdateAsync()`
  - `DeleteAsync()`
  - `Query()` - For LINQ queries

### Infrastructure Layer
- ✅ Project configured with:
  - Entity Framework Core 8.0
  - SQL Server provider
  - Azure Cosmos DB SDK

### API Layer
- ✅ ASP.NET Core Web API
- ✅ Swagger/OpenAPI support
- ✅ HTTPS enabled

## 🔧 Project Dependencies

### Domain
- **Dependencies**: None (pure business logic)

### Application
- **Dependencies**: Domain

### Infrastructure
- **Dependencies**: Domain, Application
- **NuGet Packages**:
  - Microsoft.EntityFrameworkCore (8.0.0)
  - Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
  - Microsoft.Azure.Cosmos (3.38.1)

### API
- **Dependencies**: Application, Infrastructure
- **NuGet Packages**:
  - Swashbuckle.AspNetCore (6.5.0)

## 📝 Next Steps

### 1. Add Your First Entity

Create a new entity in `src/Domain/Entities/`:

```csharp
namespace Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
```

### 2. Implement Repository

In `src/Infrastructure/Repositories/`, create:

```csharp
using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class ProductRepository : IRepository<Product>
{
    // Implement interface methods here
}
```

### 3. Create a Controller

In `src/API/Controllers/`, create:

```csharp
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _repository;

    public ProductsController(IRepository<Product> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }
}
```

### 4. Configure Dependency Injection

In `src/API/Program.cs`, add:

```csharp
// Add this before var app = builder.Build();
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
```

## 🏗️ Clean Architecture Benefits

1. **Testability**: Easy to unit test each layer
2. **Maintainability**: Clear separation of concerns
3. **Flexibility**: Easy to swap implementations
4. **Independence**: Business logic independent of frameworks
5. **Scalability**: Easy to add new features

## 📚 Common Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API
cd src/API && dotnet run

# Run with watch (auto-reload)
cd src/API && dotnet watch run

# Add new package
cd src/Infrastructure
dotnet add package PackageName

# Create new project
dotnet new classlib -n ProjectName

# Add project to solution
dotnet sln add path/to/Project.csproj
```

## 🎯 Architecture Guidelines

### ✅ Do's
- Keep Domain layer free of external dependencies
- Use interfaces in Application layer
- Implement interfaces in Infrastructure layer
- Use dependency injection
- Follow SOLID principles

### ❌ Don'ts
- Don't reference Infrastructure from Domain
- Don't put business logic in Controllers
- Don't use concrete implementations in Application
- Don't skip interfaces for testability

## 📖 Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

## 🆘 Troubleshooting

### Build Errors
```bash
dotnet clean
dotnet restore
dotnet build
```

### Port Already in Use
Edit `src/API/Properties/launchSettings.json` to change ports

### Missing Dependencies
```bash
dotnet restore
```

---

**You're all set!** Start building your application following Clean Architecture principles. 🚀
