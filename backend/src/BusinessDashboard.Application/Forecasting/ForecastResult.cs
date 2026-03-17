using BusinessDashboard.Infrastructure.Dashboard;

namespace BusinessDashboard.Application.Forecasting;

public sealed class ForecastResult
{
    public string ModelKey { get; init; } = string.Empty;
    public string ModelLabel { get; init; } = string.Empty;
    public int Priority { get; init; }
    public bool UsedHistoricalComparisons { get; init; }
    public int BasisYearsCount { get; init; }
    public DashboardPerformanceSeriesLineDto Series { get; init; } = new();
}
