using BusinessDashboard.Infrastructure.Dashboard;
using System.Globalization;

namespace BusinessDashboard.Application.Forecasting.Strategies;

public sealed class HistoricalAverageForecastStrategy : IForecastStrategy
{
    public string Key => "historical_average";
    public string Label => "Historical average";

    public int GetPriority(ForecastRequest request)
        => request.HasFixedRange && request.CurrentSeries.Points.Count > 0 && request.YearComparisonSeries.Count > 0 ? 200 : -1;

    public ForecastResult? BuildForecast(ForecastRequest request)
    {
        var currentSlotStart = GetPeriodStart(DateTime.UtcNow, request.GroupBy, request.TzOffsetMinutes);
        var targetPoints = request.CurrentSeries.Points
            .Where(p => p.PeriodStart is not null && p.PeriodStart > currentSlotStart)
            .ToList();

        if (targetPoints.Count == 0)
            return null;

        var forecastPoints = new List<DashboardPerformancePointDto>(targetPoints.Count);

        foreach (var targetPoint in targetPoints)
        {
            var revenue = TryGetHistoricalAverageRevenue(request.YearComparisonSeries, targetPoint.AxisIndex);
            if (revenue is null)
                continue;

            forecastPoints.Add(new DashboardPerformancePointDto
            {
                AxisIndex = targetPoint.AxisIndex,
                AxisLabel = targetPoint.AxisLabel,
                PeriodStart = targetPoint.PeriodStart,
                Revenue = revenue.Value,
                Costs = 0m,
                Gains = 0m,
                MarginPct = 0m,
                AvgTicket = 0m,
                SalesCount = 0
            });
        }

        if (forecastPoints.Count == 0)
            return null;

        return new ForecastResult
        {
            ModelKey = Key,
            ModelLabel = Label,
            Priority = GetPriority(request),
            UsedHistoricalComparisons = true,
            BasisYearsCount = request.YearComparisonSeries.Count,
            Series = new DashboardPerformanceSeriesLineDto
            {
                Id = "forecast",
                Label = $"Forecast ({request.YearComparisonSeries.Count}y avg)",
                Kind = "forecast",
                Points = forecastPoints
            }
        };
    }

    private static decimal? TryGetHistoricalAverageRevenue(
        IReadOnlyList<DashboardPerformanceSeriesLineDto> yearComparisonSeries,
        int axisIndex)
    {
        var values = yearComparisonSeries
            .Select(series => new
            {
                YearOffset = TryParseYearOffset(series.Id),
                Point = series.Points.FirstOrDefault(point => point.AxisIndex == axisIndex)
            })
            .Where(x => x.YearOffset is not null && x.Point is not null)
            .Select(x => x.Point!.Revenue)
            .ToList();

        if (values.Count == 0)
            return null;

        return Math.Round(values.Average(), 2);
    }

    private static int? TryParseYearOffset(string id)
    {
        const string yearPrefix = "year-";
        const string forecastYearPrefix = "forecast-year-";

        if (id.StartsWith(forecastYearPrefix, StringComparison.Ordinal) &&
            int.TryParse(id[forecastYearPrefix.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var forecastYearOffset) &&
            forecastYearOffset > 0)
        {
            return forecastYearOffset;
        }

        if (id.StartsWith(yearPrefix, StringComparison.Ordinal) &&
            int.TryParse(id[yearPrefix.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var yearOffset) &&
            yearOffset > 0)
        {
            return yearOffset;
        }

        return null;
    }

    private static DateTime GetPeriodStart(DateTime dt, string groupBy, int tzOffsetMinutes)
    {
        var localDt = dt.ToUniversalTime().AddMinutes(-tzOffsetMinutes);

        DateTime periodStart = groupBy switch
        {
            "day" => new DateTime(localDt.Year, localDt.Month, localDt.Day, 0, 0, 0, DateTimeKind.Unspecified),
            "month" => new DateTime(localDt.Year, localDt.Month, 1, 0, 0, 0, DateTimeKind.Unspecified),
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy))
        };

        return DateTime.SpecifyKind(periodStart.AddMinutes(tzOffsetMinutes), DateTimeKind.Utc);
    }
}
