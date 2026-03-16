namespace BusinessDashboard.Infrastructure.Dashboard;

public class DashboardPerformanceSeriesLineDto
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public IReadOnlyList<DashboardPerformancePointDto> Points { get; init; } = Array.Empty<DashboardPerformancePointDto>();
}
