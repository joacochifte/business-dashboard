using BusinessDashboard.Application.Forecasting.Strategies;

namespace BusinessDashboard.Application.Forecasting;

public sealed class ForecastStrategySelector : IForecastStrategySelector
{
    public IForecastStrategy? Select(ForecastRequest request, IReadOnlyList<IForecastStrategy> strategies)
    {
        if (!string.IsNullOrWhiteSpace(request.RequestedModelKey))
        {
            var requested = strategies.FirstOrDefault(strategy =>
                string.Equals(strategy.Key, request.RequestedModelKey, StringComparison.OrdinalIgnoreCase));

            if (requested is null)
                return null;

            return requested.GetPriority(request) >= 0 ? requested : null;
        }

        return strategies
            .Select(strategy => new { Strategy = strategy, Priority = strategy.GetPriority(request) })
            .Where(x => x.Priority >= 0)
            .OrderByDescending(x => x.Priority)
            .Select(x => x.Strategy)
            .FirstOrDefault();
    }
}
