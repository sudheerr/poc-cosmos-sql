namespace Domain.Entities;

/// <summary>
/// Example entity - Product (can be used with both Cosmos DB and SQL Server)
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
