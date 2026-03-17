using BusinessDashboard.Infrastructure.Dashboard;
using System.Globalization;

namespace BusinessDashboard.Application.Forecasting.Strategies;

public sealed class YearRegressionForecastStrategy : IForecastStrategy
{
    public string Key => "year_regression";
    public string Label => "Year regression";

    public int GetPriority(ForecastRequest request)
        => request.HasFixedRange && request.CurrentSeries.Points.Count > 0 ? 100 : -1;

    public ForecastResult? BuildForecast(ForecastRequest request)
    {
        var currentSlotStart = GetPeriodStart(DateTime.UtcNow, request.GroupBy, request.TzOffsetMinutes);
        var source = request.CurrentSeries.Points
            .Where(p => p.PeriodStart is null || p.PeriodStart <= currentSlotStart)
            .ToList();

        if (source.Count == 0)
            return null;

        var targetPoints = request.CurrentSeries.Points
            .Where(p => p.PeriodStart is not null && p.PeriodStart > currentSlotStart)
            .ToList();

        if (targetPoints.Count == 0)
            return null;

        var count = source.Count;
        var sumX = source.Sum(p => (decimal)p.AxisIndex);
        var sumY = source.Sum(p => p.Revenue);
        var sumXY = source.Sum(p => (decimal)p.AxisIndex * p.Revenue);
        var sumXX = source.Sum(p => (decimal)p.AxisIndex * p.AxisIndex);
        var denominator = (count * sumXX) - (sumX * sumX);
        var slope = denominator == 0m ? 0m : ((count * sumXY) - (sumX * sumY)) / denominator;
        var intercept = count == 0 ? 0m : (sumY - (slope * sumX)) / count;

        var forecastPoints = new List<DashboardPerformancePointDto>(targetPoints.Count);

        foreach (var targetPoint in targetPoints)
        {
            var revenue = Math.Max(
                0m,
                TryPredictRevenueFromYearRegression(request.YearComparisonSeries, targetPoint)
                    ?? Math.Round(intercept + (slope * targetPoint.AxisIndex), 2));

            forecastPoints.Add(new DashboardPerformancePointDto
            {
                AxisIndex = targetPoint.AxisIndex,
                AxisLabel = targetPoint.AxisLabel,
                PeriodStart = targetPoint.PeriodStart,
                Revenue = revenue,
                Costs = 0m,
                Gains = 0m,
                MarginPct = 0m,
                AvgTicket = 0m,
                SalesCount = 0
            });
        }

        return new ForecastResult
        {
            ModelKey = Key,
            ModelLabel = Label,
            Priority = GetPriority(request),
            UsedHistoricalComparisons = request.YearComparisonSeries.Count > 0,
            BasisYearsCount = request.YearComparisonSeries.Count,
            Series = new DashboardPerformanceSeriesLineDto
            {
                Id = "forecast",
                Label = request.YearComparisonSeries.Count > 0 ? $"Forecast ({request.YearComparisonSeries.Count}y regression)" : "Forecast",
                Kind = "forecast",
                Points = forecastPoints
            }
        };
    }

    private static decimal? TryPredictRevenueFromYearRegression(
        IReadOnlyList<DashboardPerformanceSeriesLineDto> yearComparisonSeries,
        DashboardPerformancePointDto targetPoint)
    {
        if (targetPoint.PeriodStart is null)
            return null;

        var targetYear = targetPoint.PeriodStart.Value.Year;
        var samples = yearComparisonSeries
            .Select(series => new
            {
                YearOffset = TryParseYearOffset(series.Id),
                Point = series.Points.FirstOrDefault(point => point.AxisIndex == targetPoint.AxisIndex)
            })
            .Where(x => x.YearOffset is not null && x.Point is not null)
            .Select(x => new
            {
                Year = targetYear - x.YearOffset!.Value,
                Revenue = x.Point!.Revenue
            })
            .ToList();

        if (samples.Count == 0)
            return null;

        if (samples.Count == 1)
            return Math.Round(samples[0].Revenue, 2);

        var count = samples.Count;
        var sumX = samples.Sum(sample => (decimal)sample.Year);
        var sumY = samples.Sum(sample => sample.Revenue);
        var sumXY = samples.Sum(sample => (decimal)sample.Year * sample.Revenue);
        var sumXX = samples.Sum(sample => (decimal)sample.Year * sample.Year);
        var denominator = (count * sumXX) - (sumX * sumX);

        if (denominator == 0m)
            return Math.Round(samples.Average(sample => sample.Revenue), 2);

        var slope = ((count * sumXY) - (sumX * sumY)) / denominator;
        var intercept = (sumY - (slope * sumX)) / count;
        return Math.Round(intercept + (slope * targetYear), 2);
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
