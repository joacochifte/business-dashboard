using BusinessDashboard.Infrastructure.Dashboard;

namespace BusinessDashboard.Application.Forecasting;

public interface IForecastService
{
    DashboardPerformanceSeriesLineDto? BuildForecastSeries(ForecastRequest request);
}
