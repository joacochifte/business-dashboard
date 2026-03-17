using BusinessDashboard.Infrastructure.Dashboard;

namespace BusinessDashboard.Application.Forecasting.Strategies;

public interface IForecastStrategy
{
    string Key { get; }
    bool CanHandle(ForecastRequest request);
    DashboardPerformanceSeriesLineDto? BuildForecastSeries(ForecastRequest request);
}
