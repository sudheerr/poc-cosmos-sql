using Application.Interfaces;
using Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Net;

namespace Infrastructure.Repositories;

/// <summary>
/// Generic Cosmos DB repository implementation with LINQ support
/// Uses ICosmosDbService for database operations
/// </summary>
public class CosmosDbRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly Container _container;
    private readonly ICosmosDbService _cosmosDbService;

    public CosmosDbRepository(ICosmosDbService cosmosDbService, string containerName)
    {
        _cosmosDbService = cosmosDbService ?? throw new ArgumentNullException(nameof(cosmosDbService));

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));

        _container = cosmosDbService.GetContainer(containerName);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(
                id,
                new PartitionKey(id),
                cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<T>();
        var results = new List<T>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Use LINQ to Cosmos DB
        var queryable = _container.GetItemLinqQueryable<T>();
        var iterator = queryable.Where(predicate).ToFeedIterator();
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.CreatedAt = DateTime.UtcNow;

        var response = await _container.CreateItemAsync(
            entity,
            new PartitionKey(entity.Id),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.UpdatedAt = DateTime.UtcNow;

        var response = await _container.UpsertItemAsync(
            entity,
            new PartitionKey(entity.Id),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<T>(
                id,
                new PartitionKey(id),
                cancellationToken: cancellationToken);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <summary>
    /// Get IQueryable for LINQ queries
    /// Enables complex queries like: repository.Query().Where(x => x.Price > 100).OrderBy(x => x.Name)
    /// </summary>
    public IQueryable<T> Query()
    {
        return _container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: true);
    }

    /// <summary>
    /// Execute complex LINQ query asynchronously
    /// Example: await repository.QueryAsync(q => q.Where(x => x.Price > 100).OrderBy(x => x.Name))
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync(
        Func<IQueryable<T>, IQueryable<T>> queryBuilder,
        CancellationToken cancellationToken = default)
    {
        var queryable = _container.GetItemLinqQueryable<T>();
        var query = queryBuilder(queryable);
        var iterator = query.ToFeedIterator();
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    /// <summary>
    /// Get paginated results with LINQ support
    /// </summary>
    public async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(
        int pageSize,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var queryRequestOptions = new QueryRequestOptions
        {
            MaxItemCount = pageSize
        };

        var query = _container.GetItemQueryIterator<T>(
            continuationToken: continuationToken,
            requestOptions: queryRequestOptions);

        var results = new List<T>();

        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
            return (results, response.ContinuationToken);
        }

        return (results, null);
    }
}
