using BusinessDashboard.Application.Forecasting.Strategies;

namespace BusinessDashboard.Application.Forecasting;

public interface IForecastStrategySelector
{
    IForecastStrategy? Select(ForecastRequest request, IReadOnlyList<IForecastStrategy> strategies);
}
