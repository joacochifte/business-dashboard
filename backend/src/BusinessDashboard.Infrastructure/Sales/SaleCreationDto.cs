using BusinessDashboard.Infrastructure.Products;
namespace BusinessDashboard.Infrastructure.Sales;
public class SaleCreationDto
{
    public Guid ProductId { get; init; }
    public IEnumerable<SaleItemDto> Items { get; init; } = [];
    public decimal Total { get; init; }
}