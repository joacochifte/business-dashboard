namespace BusinessDashboard.Infrastructure.Products;

public class ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int? Stock { get; init; }
    public bool IsActive { get; init; }
}
