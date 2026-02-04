namespace BusinessDashboard.Infrastructure.Dashboard;
public class SalesByPeriodPointDto
{
    public DateTime PeriodStart { get; init; }
    public int SalesCount { get; init; }
    public decimal Revenue { get; init; }
}