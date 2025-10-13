using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

/// <summary>
/// Specific repository for Customer entity
/// </summary>
public class CustomerRepository : CosmosDbRepository<Customer>
{
    public CustomerRepository(ICosmosDbService cosmosDbService)
        : base(cosmosDbService, "Customers")
    {
    }

    /// <summary>
    /// Get customer by email using LINQ
    /// </summary>
    public async Task<Customer?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var customers = await FindAsync(c => c.Email == email, cancellationToken);
        return customers.FirstOrDefault();
    }

    /// <summary>
    /// Get customers by country using LINQ
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByCountryAsync(
        string country,
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(c => c.Country == country, cancellationToken);
    }

    /// <summary>
    /// Search customers with LINQ
    /// </summary>
    public async Task<IEnumerable<Customer>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await QueryAsync(query =>
            query.Where(c =>
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.Email.Contains(searchTerm))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName),
            cancellationToken);
    }
}
