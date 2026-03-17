using BusinessDashboard.Infrastructure.Dashboard;
using BusinessDashboard.Application.Forecasting.Strategies;

namespace BusinessDashboard.Application.Forecasting;

public sealed class ForecastService : IForecastService
{
    private readonly IReadOnlyList<IForecastStrategy> _strategies;

    public ForecastService(IEnumerable<IForecastStrategy> strategies)
    {
        _strategies = strategies.ToArray();
    }

    public DashboardPerformanceSeriesLineDto? BuildForecastSeries(ForecastRequest request)
    {
        var strategy = _strategies.FirstOrDefault(candidate => candidate.CanHandle(request));
        return strategy?.BuildForecastSeries(request);
    }
}
