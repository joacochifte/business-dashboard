namespace BusinessDashboard.Infrastructure.Dashboard;

public class DashboardPerformancePointDto
{
    public int AxisIndex { get; init; }
    public string AxisLabel { get; init; } = string.Empty;
    public DateTime? PeriodStart { get; init; }
    public decimal Revenue { get; init; }
    public decimal Costs { get; init; }
    public decimal Gains { get; init; }
    public decimal MarginPct { get; init; }
    public decimal AvgTicket { get; init; }
    public int SalesCount { get; init; }
}
