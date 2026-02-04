using BusinessDashboard.Infrastructure.Products;
namespace BusinessDashboard.Infrastructure.Sales;
public class SaleUpdateDto
{
    public Guid Id { get; init; }
    public IEnumerable<SaleItemDto> Items { get; init; } = [];
    public decimal Total { get; init; }
    public string? CustomerName { get; init; } = "";
    public string? PaymentMethod { get; init; } = "";
}