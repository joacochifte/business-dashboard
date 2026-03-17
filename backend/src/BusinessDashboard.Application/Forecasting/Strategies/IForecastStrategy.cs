namespace BusinessDashboard.Application.Forecasting.Strategies;

public interface IForecastStrategy
{
    string Key { get; }
    string Label { get; }
    int GetPriority(ForecastRequest request);
    ForecastResult? BuildForecast(ForecastRequest request);
}
