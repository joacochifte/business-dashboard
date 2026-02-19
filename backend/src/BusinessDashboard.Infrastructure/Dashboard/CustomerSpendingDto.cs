namespace BusinessDashboard.Infrastructure.Dashboard;

public class CustomerSpendingDto
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalSpent { get; init; }
}
