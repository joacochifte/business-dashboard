using BusinessDashboard.Infrastructure.Products;
namespace BusinessDashboard.Infrastructure.Sales;
public class SaleCreationDto
{
    public IEnumerable<SaleItemDto> Items { get; init; } = [];
    public decimal Total { get; init; }
    public Guid? CustomerId { get; init; }
    public string? PaymentMethod { get; init; } = "";
    public bool IsDebt { get; init; }
}