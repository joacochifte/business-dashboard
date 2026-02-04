using BusinessDashboard.Application.Products;
namespace BusinessDashboard.Infrastructure.Sales;
public class SaleDto
{
    public Guid ProductId { get; init; }
    public IEnumerable<ProductSummaryDto> Products { get; init; } = [];
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
}