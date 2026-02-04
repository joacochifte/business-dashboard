namespace BusinessDashboard.Infrastructure.Dashboard;

public class TopProductDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Revenue { get; init; }
}

