using BusinessDashboard.Application.Forecasting;
using BusinessDashboard.Application.Forecasting.Strategies;
using BusinessDashboard.Infrastructure.Dashboard;
using Moq;

namespace Application.Test.Forecasting;

[TestClass]
public class ForecastingTests
{
    [TestMethod]
    public void ForecastStrategySelector_WithRequestedModel_ShouldUseRequestedStrategy()
    {
        var selector = new ForecastStrategySelector();
        var request = new ForecastRequest
        {
            RequestedModelKey = "year_regression",
            HasFixedRange = true,
            CurrentSeries = BuildSeries("current", 1m, 2m, 3m)
        };

        var selected = selector.Select(request, new IForecastStrategy[]
        {
            new TestStrategy("historical_average", 200),
            new TestStrategy("year_regression", 100),
        });

        Assert.IsNotNull(selected);
        Assert.AreEqual("year_regression", selected.Key);
    }

    [TestMethod]
    public void ForecastStrategySelector_WithoutRequestedModel_ShouldUseHighestPriority()
    {
        var selector = new ForecastStrategySelector();
        var request = new ForecastRequest
        {
            HasFixedRange = true,
            CurrentSeries = BuildSeries("current", 1m, 2m, 3m)
        };

        var selected = selector.Select(request, new IForecastStrategy[]
        {
            new TestStrategy("fallback", 50),
            new TestStrategy("preferred", 250),
        });

        Assert.IsNotNull(selected);
        Assert.AreEqual("preferred", selected.Key);
    }

    [TestMethod]
    public void ForecastService_ShouldReturnSelectedStrategyResult()
    {
        var selector = new Mock<IForecastStrategySelector>(MockBehavior.Strict);
        var strategy = new Mock<IForecastStrategy>(MockBehavior.Strict);
        var request = new ForecastRequest
        {
            HasFixedRange = true,
            CurrentSeries = BuildSeries("current", 10m, 20m, 30m)
        };

        var expected = new ForecastResult
        {
            ModelKey = "historical_average",
            ModelLabel = "Historical average",
            Priority = 200,
            UsedHistoricalComparisons = true,
            BasisYearsCount = 3,
            Series = BuildSeries("forecast", 40m, 50m)
        };

        selector
            .Setup(x => x.Select(request, It.IsAny<IReadOnlyList<IForecastStrategy>>()))
            .Returns(strategy.Object);
        strategy.SetupGet(x => x.Key).Returns("historical_average");
        strategy.Setup(x => x.BuildForecast(request)).Returns(expected);

        var service = new ForecastService(new[] { strategy.Object }, selector.Object);

        var result = service.BuildForecast(request);

        Assert.IsNotNull(result);
        Assert.AreEqual("historical_average", result.ModelKey);
        Assert.AreEqual(2, result.Series.Points.Count);
    }

    [TestMethod]
    public void HistoricalAverageForecastStrategy_ShouldAveragePreviousYearsForFuturePoints()
    {
        var strategy = new HistoricalAverageForecastStrategy();
        var request = new ForecastRequest
        {
            HasFixedRange = true,
            GroupBy = "day",
            CurrentSeries = BuildSeries(
                "current",
                (1, DateTime.UtcNow.Date.AddDays(-2), 100m),
                (2, DateTime.UtcNow.Date.AddDays(-1), 120m),
                (3, DateTime.UtcNow.Date.AddDays(1), 0m),
                (4, DateTime.UtcNow.Date.AddDays(2), 0m)),
            YearComparisonSeries = new[]
            {
                BuildSeries("year-1", (1, DateTime.UtcNow.Date.AddYears(-1).AddDays(-2), 90m), (2, DateTime.UtcNow.Date.AddYears(-1).AddDays(-1), 110m), (3, DateTime.UtcNow.Date.AddYears(-1).AddDays(1), 130m), (4, DateTime.UtcNow.Date.AddYears(-1).AddDays(2), 150m)),
                BuildSeries("year-2", (1, DateTime.UtcNow.Date.AddYears(-2).AddDays(-2), 80m), (2, DateTime.UtcNow.Date.AddYears(-2).AddDays(-1), 100m), (3, DateTime.UtcNow.Date.AddYears(-2).AddDays(1), 170m), (4, DateTime.UtcNow.Date.AddYears(-2).AddDays(2), 190m)),
            }
        };

        var result = strategy.BuildForecast(request);

        Assert.IsNotNull(result);
        Assert.AreEqual("historical_average", result.ModelKey);
        Assert.AreEqual(2, result.Series.Points.Count);
        Assert.AreEqual(150m, result.Series.Points[0].Revenue);
        Assert.AreEqual(170m, result.Series.Points[1].Revenue);
    }

    [TestMethod]
    public void YearRegressionForecastStrategy_ShouldReturnMetadataAndForecastPoints()
    {
        var strategy = new YearRegressionForecastStrategy();
        var today = DateTime.UtcNow.Date;
        var request = new ForecastRequest
        {
            HasFixedRange = true,
            GroupBy = "day",
            CurrentSeries = BuildSeries(
                "current",
                (1, today.AddDays(-2), 100m),
                (2, today.AddDays(-1), 120m),
                (3, today.AddDays(1), 0m)),
            YearComparisonSeries = new[]
            {
                BuildSeries("year-1", (1, today.AddYears(-1).AddDays(-2), 80m), (2, today.AddYears(-1).AddDays(-1), 100m), (3, today.AddYears(-1).AddDays(1), 140m)),
                BuildSeries("year-2", (1, today.AddYears(-2).AddDays(-2), 90m), (2, today.AddYears(-2).AddDays(-1), 110m), (3, today.AddYears(-2).AddDays(1), 150m)),
            }
        };

        var result = strategy.BuildForecast(request);

        Assert.IsNotNull(result);
        Assert.AreEqual("year_regression", result.ModelKey);
        Assert.AreEqual("Year regression", result.ModelLabel);
        Assert.IsTrue(result.UsedHistoricalComparisons);
        Assert.AreEqual(2, result.BasisYearsCount);
        Assert.AreEqual(1, result.Series.Points.Count);
        Assert.IsTrue(result.Series.Points[0].Revenue > 0m);
    }

    private static DashboardPerformanceSeriesLineDto BuildSeries(string id, params decimal[] revenues)
    {
        var today = DateTime.UtcNow.Date;
        return new DashboardPerformanceSeriesLineDto
        {
            Id = id,
            Label = id,
            Kind = id,
            Points = revenues
                .Select((revenue, index) => new DashboardPerformancePointDto
                {
                    AxisIndex = index + 1,
                    AxisLabel = (index + 1).ToString("00"),
                    PeriodStart = today.AddDays(index - revenues.Length).ToUniversalTime(),
                    Revenue = revenue
                })
                .ToList()
        };
    }

    private static DashboardPerformanceSeriesLineDto BuildSeries(string id, params (int AxisIndex, DateTime PeriodStart, decimal Revenue)[] points)
    {
        return new DashboardPerformanceSeriesLineDto
        {
            Id = id,
            Label = id,
            Kind = id,
            Points = points
                .Select(point => new DashboardPerformancePointDto
                {
                    AxisIndex = point.AxisIndex,
                    AxisLabel = point.AxisIndex.ToString("00"),
                    PeriodStart = DateTime.SpecifyKind(point.PeriodStart, DateTimeKind.Utc),
                    Revenue = point.Revenue
                })
                .ToList()
        };
    }

    private sealed class TestStrategy : IForecastStrategy
    {
        private readonly int _priority;

        public TestStrategy(string key, int priority)
        {
            Key = key;
            Label = key;
            _priority = priority;
        }

        public string Key { get; }
        public string Label { get; }

        public int GetPriority(ForecastRequest request) => _priority;

        public ForecastResult? BuildForecast(ForecastRequest request)
            => new()
            {
                ModelKey = Key,
                ModelLabel = Label,
                Priority = _priority,
                Series = BuildSeries("forecast", 1m)
            };
    }
}
