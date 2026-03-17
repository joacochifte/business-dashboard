using BusinessDashboard.Infrastructure.Dashboard;
using BusinessDashboard.Application.Forecasting.Strategies;

namespace BusinessDashboard.Application.Forecasting;

public sealed class ForecastService : IForecastService
{
    private readonly IReadOnlyList<IForecastStrategy> _strategies;
    private readonly IForecastStrategySelector _selector;

    public ForecastService(IEnumerable<IForecastStrategy> strategies, IForecastStrategySelector selector)
    {
        _strategies = strategies.ToArray();
        _selector = selector;
    }

    public ForecastResult? BuildForecast(ForecastRequest request)
    {
        var strategy = _selector.Select(request, _strategies);
        return strategy?.BuildForecast(request);
    }
}
