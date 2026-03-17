namespace BusinessDashboard.Application.Forecasting;

public interface IForecastService
{
    ForecastResult? BuildForecast(ForecastRequest request);
}
