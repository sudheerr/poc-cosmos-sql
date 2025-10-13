# Test Projects

This directory contains all test projects for the CosmosDbSqlApi solution.

## Test Projects

### 1. Domain.Tests
Tests for domain entities and business logic.

**Test Coverage:**
- Entity validation
- Property initialization
- Business rule enforcement

**Run:**
```bash
dotnet test tests/Domain.Tests
```

### 2. API.Tests
Unit and integration tests for API controllers.

**Test Coverage:**
- Controller unit tests (using Moq)
- Integration tests (using WebApplicationFactory)
- HTTP endpoint testing
- Response validation

**Run:**
```bash
# All API tests
dotnet test tests/API.Tests

# Only unit tests
dotnet test tests/API.Tests --filter "FullyQualifiedName~Controllers"

# Only integration tests
dotnet test tests/API.Tests --filter "FullyQualifiedName~Integration"
```

### 3. Infrastructure.Tests
Integration tests for data access layer.

**Test Coverage:**
- Repository CRUD operations
- LINQ query support
- Pagination
- Unit of Work pattern
- Transactions

**Run:**
```bash
dotnet test tests/Infrastructure.Tests
```

## Quick Start

### Install Dependencies
```bash
# From solution root
dotnet restore
```

### Run All Tests
```bash
dotnet test
```

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Statistics

Current test coverage includes:

- **Domain Tests:** 10+ tests covering all entities
- **Controller Unit Tests:** 15+ tests using Moq for mocking
- **Repository Integration Tests:** 25+ tests with InMemory database
- **API Integration Tests:** 15+ end-to-end tests

## More Information

See [TESTING-GUIDE.md](../TESTING-GUIDE.md) for detailed testing documentation.
