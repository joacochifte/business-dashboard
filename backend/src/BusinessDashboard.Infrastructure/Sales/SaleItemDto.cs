namespace BusinessDashboard.Infrastructure.Sales;
public class SaleItemDto
{
    public Guid ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal? SpecialPrice { get; init; }
}