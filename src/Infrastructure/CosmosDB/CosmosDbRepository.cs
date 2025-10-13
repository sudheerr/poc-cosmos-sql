using System.Linq.Expressions;
using System.Net;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Infrastructure.CosmosDB;

/// <summary>
/// Generic Cosmos DB repository implementation using singleton Cosmos client
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class CosmosDbRepository<T> : IDataRepository<T> where T : BaseEntity
{
    private readonly Container _container;

    public CosmosDbRepository(CosmosDbService cosmosDbService)
    {
        _container = cosmosDbService.GetContainerAsync().GetAwaiter().GetResult();
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        var response = await _container.CreateItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var tasks = entities.Select(entity =>
        {
            entity.CreatedAt = DateTime.UtcNow;
            return _container.CreateItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken);
        });

        var responses = await Task.WhenAll(tasks);
        return responses.Select(r => r.Resource);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken);
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

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
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

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var queryable = _container.GetItemLinqQueryable<T>();
        var iterator = queryable.Where(predicate).Take(1).ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }

        return null;
    }

    public IQueryable<T> Query()
    {
        return _container.GetItemLinqQueryable<T>();
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var response = await _container.UpsertItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var tasks = entities.Select(entity =>
        {
            entity.UpdatedAt = DateTime.UtcNow;
            return _container.UpsertItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken);
        });

        var responses = await Task.WhenAll(tasks);
        return responses.Select(r => r.Resource);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(entity.Id, cancellationToken);
    }

    public async Task<int> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var tasks = entities.Select(entity => DeleteAsync(entity.Id, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.Count(r => r);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition($"SELECT VALUE COUNT(1) FROM c");
        var iterator = _container.GetItemQueryIterator<int>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }

        return 0;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var queryable = _container.GetItemLinqQueryable<T>();
        var iterator = queryable.Where(predicate).Count().ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }

        return 0;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var item = await FirstOrDefaultAsync(predicate, cancellationToken);
        return item != null;
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _container.GetItemLinqQueryable<T>();

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        // Get total count
        var countIterator = queryable.Count().ToFeedIterator();
        var totalCount = 0;
        if (countIterator.HasMoreResults)
        {
            var countResponse = await countIterator.ReadNextAsync(cancellationToken);
            totalCount = countResponse.FirstOrDefault();
        }

        // Get paged items
        var iterator = queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToFeedIterator();

        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return (results, totalCount);
    }
}
