# Clean Architecture Solution - .NET 8

## Overview
This is a .NET 8 solution implementing Clean Architecture principles with support for multiple data sources.

## Solution Structure

```
CleanArchitecture.sln
├── src/
│   ├── Domain/              # Core business entities and logic
│   │   └── Entities/       # Domain entities
│   ├── Application/         # Application business rules
│   │   └── Interfaces/     # Repository interfaces
│   ├── Infrastructure/      # External concerns (Database, APIs)
│   │   ├── Persistence/    # Database implementations
│   │   └── Services/       # External services
│   └── API/                # Web API presentation layer
│       ├── Controllers/    # API endpoints
│       └── Program.cs      # Application entry point
```

## Clean Architecture Layers

### 1. Domain Layer (Core)
- **Purpose**: Contains enterprise business rules and entities
- **Dependencies**: None (completely independent)
- **Contents**:
  - Entities
  - Value Objects
  - Domain Events
  - Interfaces (optional)

### 2. Application Layer
- **Purpose**: Contains application business rules
- **Dependencies**: Domain layer only
- **Contents**:
  - Interfaces (IRepository, IService)
  - DTOs
  - Application Services
  - Use Cases/Commands/Queries

### 3. Infrastructure Layer
- **Purpose**: Implements interfaces defined in Application layer
- **Dependencies**: Domain and Application layers
- **Contents**:
  - Database Context (EF Core)
  - Repository Implementations
  - External API clients
  - File System access
  - Email services

### 4. API/Presentation Layer
- **Purpose**: User interface and API endpoints
- **Dependencies**: Application and Infrastructure layers
- **Contents**:
  - Controllers
  - Middleware
  - Filters
  - Program.cs / Startup configuration

## Dependency Flow

```
API --> Application --> Domain
 |          |
 v          v
Infrastructure
```

## Key Principles

1. **Dependency Inversion**: High-level modules don't depend on low-level modules
2. **Separation of Concerns**: Each layer has a specific responsibility
3. **Testability**: Easy to unit test due to dependency injection
4. **Independence**: Business logic is independent of frameworks and UI

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Build the Solution
```bash
dotnet restore
dotnet build
```

### Run the API
```bash
cd src/API
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7xxx
- HTTP: http://localhost:5xxx
- Swagger: https://localhost:7xxx/swagger

## Project References

- **Domain**: No dependencies
- **Application**: References Domain
- **Infrastructure**: References Domain and Application
- **API**: References Application and Infrastructure

## Next Steps

1. Add your domain entities in `src/Domain/Entities`
2. Define repository interfaces in `src/Application/Interfaces`
3. Implement repositories in `src/Infrastructure`
4. Create controllers in `src/API/Controllers`
5. Configure dependency injection in `Program.cs`

## Best Practices

- Keep Domain layer pure (no external dependencies)
- Use interfaces for all external dependencies
- Implement repository pattern for data access
- Use DTOs for data transfer between layers
- Apply SOLID principles throughout
