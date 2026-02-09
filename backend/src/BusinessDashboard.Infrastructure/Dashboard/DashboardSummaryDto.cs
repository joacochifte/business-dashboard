namespace BusinessDashboard.Infrastructure.Dashboard;

public class DashboardSummaryDto
{
    public decimal RevenueTotal { get; init; }
    public decimal Gains { get; init; }
    public int SalesCount { get; init; }
    public decimal AvgTicket { get; init; }
}
