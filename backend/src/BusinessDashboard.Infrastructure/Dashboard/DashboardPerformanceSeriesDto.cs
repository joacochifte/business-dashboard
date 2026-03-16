namespace BusinessDashboard.Infrastructure.Dashboard;

public class DashboardPerformanceSeriesDto
{
    public string GroupBy { get; init; } = string.Empty;
    public string AxisMode { get; init; } = string.Empty;
    public DashboardPerformanceSeriesLineDto CurrentSeries { get; init; } = new();
    public IReadOnlyList<DashboardPerformanceSeriesLineDto> ComparisonSeries { get; init; } = Array.Empty<DashboardPerformanceSeriesLineDto>();
    public DashboardPerformanceSeriesLineDto? ForecastSeries { get; init; }
}
