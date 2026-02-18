using BusinessDashboard.Infrastructure.Products;
namespace BusinessDashboard.Infrastructure.Sales;
public class SaleDto
{
    public Guid Id { get; init; }
    public IEnumerable<SaleItemDto> Items { get; init; } = [];
    public Guid? CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public string? PaymentMethod { get; init; } = "";
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
}