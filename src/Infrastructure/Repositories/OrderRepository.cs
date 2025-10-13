using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

/// <summary>
/// Specific repository for Order entity
/// </summary>
public class OrderRepository : CosmosDbRepository<Order>
{
    public OrderRepository(ICosmosDbService cosmosDbService)
        : base(cosmosDbService, "Orders")
    {
    }

    /// <summary>
    /// Get orders by customer using LINQ
    /// </summary>
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await QueryAsync(query =>
            query.Where(o => o.CustomerId == customerId)
                 .OrderByDescending(o => o.OrderDate),
            cancellationToken);
    }

    /// <summary>
    /// Get orders by status using LINQ
    /// </summary>
    public async Task<IEnumerable<Order>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(o => o.Status == status, cancellationToken);
    }

    /// <summary>
    /// Get orders within date range using LINQ
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await QueryAsync(query =>
            query.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                 .OrderByDescending(o => o.OrderDate),
            cancellationToken);
    }

    /// <summary>
    /// Get pending orders using LINQ
    /// </summary>
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        return await FindAsync(o => o.Status == "Pending", cancellationToken);
    }

    /// <summary>
    /// Calculate total revenue using LINQ
    /// </summary>
    public async Task<decimal> GetTotalRevenueAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var orders = await QueryAsync(query =>
        {
            var q = query.Where(o => o.Status == "Completed");

            if (startDate.HasValue)
                q = q.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(o => o.OrderDate <= endDate.Value);

            return q;
        }, cancellationToken);

        return orders.Sum(o => o.TotalAmount);
    }
}
