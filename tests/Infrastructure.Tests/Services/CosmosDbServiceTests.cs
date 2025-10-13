using Application.Interfaces;
using Infrastructure.CosmosDB;
using Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services;

public class CosmosDbServiceTests
{
    private readonly Mock<CosmosClient> _mockCosmosClient;
    private readonly Mock<Database> _mockDatabase;
    private readonly Mock<Container> _mockContainer;
    private readonly CosmosDbSettings _settings;
    private readonly ICosmosDbService _cosmosDbService;

    public CosmosDbServiceTests()
    {
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockDatabase = new Mock<Database>();
        _mockContainer = new Mock<Container>();

        _settings = new CosmosDbSettings
        {
            DatabaseName = "TestDB",
            ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=testkey==",
            Throughput = 400
        };

        _mockCosmosClient.Setup(c => c.GetDatabase(It.IsAny<string>()))
            .Returns(_mockDatabase.Object);

        _mockDatabase.Setup(d => d.GetContainer(It.IsAny<string>()))
            .Returns(_mockContainer.Object);

        _cosmosDbService = new CosmosDbService(_mockCosmosClient.Object, _settings);
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CosmosDbService(null!, _settings));
    }

    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CosmosDbService(_mockCosmosClient.Object, null!));
    }

    [Fact]
    public void Client_ShouldReturnCosmosClient()
    {
        // Act
        var client = _cosmosDbService.Client;

        // Assert
        Assert.NotNull(client);
        Assert.Equal(_mockCosmosClient.Object, client);
    }

    [Fact]
    public void DatabaseName_ShouldReturnConfiguredName()
    {
        // Act
        var databaseName = _cosmosDbService.DatabaseName;

        // Assert
        Assert.Equal("TestDB", databaseName);
    }

    [Fact]
    public void GetDatabase_ShouldReturnDatabase()
    {
        // Act
        var database = _cosmosDbService.GetDatabase();

        // Assert
        Assert.NotNull(database);
        _mockCosmosClient.Verify(c => c.GetDatabase("TestDB"), Times.Once);
    }

    [Fact]
    public void GetContainer_WithValidName_ShouldReturnContainer()
    {
        // Arrange
        var containerName = "Products";

        // Act
        var container = _cosmosDbService.GetContainer(containerName);

        // Assert
        Assert.NotNull(container);
        _mockDatabase.Verify(d => d.GetContainer(containerName), Times.Once);
    }

    [Fact]
    public void GetContainer_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _cosmosDbService.GetContainer(null!));
    }

    [Fact]
    public void GetContainer_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _cosmosDbService.GetContainer(string.Empty));
    }

    [Fact]
    public async Task CreateDatabaseIfNotExistsAsync_ShouldCallCosmosClient()
    {
        // Arrange
        var mockDatabaseResponse = new Mock<DatabaseResponse>();
        mockDatabaseResponse.Setup(r => r.Database).Returns(_mockDatabase.Object);

        _mockCosmosClient.Setup(c => c.CreateDatabaseIfNotExistsAsync(
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDatabaseResponse.Object);

        // Act
        var result = await _cosmosDbService.CreateDatabaseIfNotExistsAsync(400);

        // Assert
        Assert.NotNull(result);
        _mockCosmosClient.Verify(c => c.CreateDatabaseIfNotExistsAsync(
            "TestDB",
            400,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task CreateContainerIfNotExistsAsync_WithValidParameters_ShouldCreateContainer()
    {
        // Arrange
        var containerName = "Products";
        var partitionKeyPath = "/category";
        var mockContainerResponse = new Mock<ContainerResponse>();
        mockContainerResponse.Setup(r => r.Container).Returns(_mockContainer.Object);

        _mockDatabase.Setup(d => d.CreateContainerIfNotExistsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContainerResponse.Object);

        // Act
        var result = await _cosmosDbService.CreateContainerIfNotExistsAsync(
            containerName, partitionKeyPath, 400);

        // Assert
        Assert.NotNull(result);
        _mockDatabase.Verify(d => d.CreateContainerIfNotExistsAsync(
            containerName,
            partitionKeyPath,
            400,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task CreateContainerIfNotExistsAsync_WithNullContainerName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _cosmosDbService.CreateContainerIfNotExistsAsync(null!, "/id"));
    }

    [Fact]
    public async Task CreateContainerIfNotExistsAsync_WithNullPartitionKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _cosmosDbService.CreateContainerIfNotExistsAsync("Products", null!));
    }
}
