using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

/// <summary>
/// Specific repository for Product entity
/// Inherits all generic functionality and can add product-specific methods
/// </summary>
public class ProductRepository : CosmosDbRepository<Product>
{
    public ProductRepository(ICosmosDbService cosmosDbService)
        : base(cosmosDbService, "Products")
    {
    }

    /// <summary>
    /// Example of product-specific method using LINQ
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.Category == category, cancellationToken);
    }

    /// <summary>
    /// Get products in stock using LINQ
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsInStockAsync(
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.Stock > 0 && p.IsActive, cancellationToken);
    }

    /// <summary>
    /// Complex LINQ query example
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(
        string? searchTerm,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken = default)
    {
        return await QueryAsync(query =>
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                        p.Description.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            return query.Where(p => p.IsActive)
                       .OrderBy(p => p.Name);
        }, cancellationToken);
    }
}
