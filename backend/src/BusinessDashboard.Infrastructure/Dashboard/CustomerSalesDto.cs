namespace BusinessDashboard.Infrastructure.Dashboard;

public class CustomerSalesDto
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int SalesCount { get; init; }
}
