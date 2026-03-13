namespace BusinessDashboard.Infrastructure.Dashboard;

public class DashboardComparisonDto
{
    public decimal? RevenueDeltaPct { get; init; }
    public decimal? CostsDeltaPct { get; init; }
    public decimal? GainsDeltaPct { get; init; }
    public decimal? SalesCountDeltaPct { get; init; }
    public decimal? UnitsSoldDeltaPct { get; init; }
}
