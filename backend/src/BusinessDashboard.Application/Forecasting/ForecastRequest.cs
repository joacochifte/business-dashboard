using BusinessDashboard.Infrastructure.Dashboard;

namespace BusinessDashboard.Application.Forecasting;

public sealed class ForecastRequest
{
    public DashboardPerformanceSeriesLineDto CurrentSeries { get; init; } = new();
    public string? RequestedModelKey { get; init; }
    public string GroupBy { get; init; } = string.Empty;
    public int TzOffsetMinutes { get; init; }
    public bool HasFixedRange { get; init; }
    public IReadOnlyList<DashboardPerformanceSeriesLineDto> YearComparisonSeries { get; init; } = Array.Empty<DashboardPerformanceSeriesLineDto>();
}
