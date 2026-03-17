using BusinessDashboard.Application.Dashboard;
using BusinessDashboard.Application.Forecasting;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Domain.Products;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Infrastructure.Dashboard;
using BusinessDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Test.Dashboard;

[TestClass]
public class DashboardServiceTests
{
    [TestMethod]
    public async Task GetSummaryAsync_ShouldCalculateRevenueGainsSalesCountAndAverageTicket()
    {
        await using var db = CreateDbContext();
        var product = new Product("Monitor", 100m, initialStock: 10);
        db.Products.Add(product);

        db.Sales.AddRange(
            new Sale([new SaleItem(product.Id, 1, 100m)], createdAt: Utc(2026, 3, 2, 10, 0)),
            new Sale([new SaleItem(product.Id, 2, 50m)], createdAt: Utc(2026, 3, 2, 15, 0)),
            new Sale([new SaleItem(product.Id, 1, 80m)], isDebt: true, createdAt: Utc(2026, 3, 2, 18, 0)),
            new Sale([new SaleItem(product.Id, 1, 40m)], createdAt: Utc(2026, 3, 1, 12, 0)));

        db.Costs.AddRange(
            new Cost("Rent", 120m, Utc(2026, 3, 2, 8, 0)),
            new Cost("Ads", 30m, Utc(2026, 3, 2, 9, 0)),
            new Cost("Previous", 25m, Utc(2026, 3, 1, 9, 0)));
        await db.SaveChangesAsync();

        var service = new DashboardService(db, new StubForecastService());

        var summary = await service.GetSummaryAsync(
            from: Utc(2026, 3, 2, 0, 0),
            to: Utc(2026, 3, 2, 23, 59));

        Assert.AreEqual(200m, summary.RevenueTotal);
        Assert.AreEqual(50m, summary.Gains);
        Assert.AreEqual(2, summary.SalesCount);
        Assert.AreEqual(100m, summary.AvgTicket);
    }

    [TestMethod]
    public async Task GetSummaryAsync_WithEmptyPeriod_ShouldReturnZeros()
    {
        await using var db = CreateDbContext();
        var service = new DashboardService(db, new StubForecastService());

        var summary = await service.GetSummaryAsync(
            from: Utc(2026, 5, 1, 0, 0),
            to: Utc(2026, 5, 31, 23, 59));

        Assert.AreEqual(0m, summary.RevenueTotal);
        Assert.AreEqual(0m, summary.Gains);
        Assert.AreEqual(0, summary.SalesCount);
        Assert.AreEqual(0m, summary.AvgTicket);
    }

    [TestMethod]
    public async Task GetOverviewAsync_ShouldCalculateMetricsComparisonAndAlerts()
    {
        await using var db = CreateDbContext();
        var customer = new Customer("Alice Buyer", email: "alice@example.com");
        var topProduct = new Product("Laptop Stand", 50m, initialStock: 0);
        var lowStockProduct = new Product("Mouse Pad", 20m, initialStock: 3);
        var healthyProduct = new Product("Desk Lamp", 30m, initialStock: 10);

        var previousSale = new Sale(
            [new SaleItem(healthyProduct.Id, 1, 80m)],
            customerId: customer.Id,
            paymentMethod: "Card",
            createdAt: Utc(2026, 3, 1, 12, 0));
        previousSale.SetCustomer(customer);

        var currentSale = new Sale(
            [new SaleItem(topProduct.Id, 2, 50m)],
            customerId: customer.Id,
            paymentMethod: "Cash",
            createdAt: Utc(2026, 3, 2, 10, 0));
        currentSale.SetCustomer(customer);

        var debtSale = new Sale(
            [new SaleItem(lowStockProduct.Id, 1, 50m)],
            customerId: customer.Id,
            paymentMethod: "Cash",
            isDebt: true,
            createdAt: Utc(2026, 3, 2, 15, 0));
        debtSale.SetCustomer(customer);

        db.Customers.Add(customer);
        db.Products.AddRange(topProduct, lowStockProduct, healthyProduct);
        db.Sales.AddRange(previousSale, currentSale, debtSale);
        db.Costs.AddRange(
            new Cost("Hosting", 40m, Utc(2026, 3, 1, 9, 0)),
            new Cost("Ads", 160m, Utc(2026, 3, 2, 9, 0)));
        await db.SaveChangesAsync();

        var service = new DashboardService(db, new StubForecastService());

        var overview = await service.GetOverviewAsync(
            from: Utc(2026, 3, 2, 0, 0),
            to: Utc(2026, 3, 2, 23, 59));

        Assert.AreEqual(100m, overview.RevenueTotal);
        Assert.AreEqual(160m, overview.CostsTotal);
        Assert.AreEqual(-60m, overview.Gains);
        Assert.AreEqual(-60m, overview.MarginPct);
        Assert.AreEqual(1, overview.SalesCount);
        Assert.AreEqual(2, overview.UnitsSold);
        Assert.AreEqual(100m, overview.AvgTicket);
        Assert.AreEqual(50m, overview.DebtsTotal);
        Assert.AreEqual(33.33m, overview.DebtRatioPct);
        Assert.AreEqual(1, overview.LowStockCount);
        Assert.AreEqual(1, overview.OutOfStockCount);

        Assert.IsNotNull(overview.TopCustomer);
        Assert.AreEqual(customer.Id, overview.TopCustomer.CustomerId);
        Assert.AreEqual("Alice Buyer", overview.TopCustomer.CustomerName);
        Assert.AreEqual(100m, overview.TopCustomer.TotalSpent);

        Assert.IsNotNull(overview.TopProductByQuantity);
        Assert.AreEqual(topProduct.Id, overview.TopProductByQuantity.ProductId);
        Assert.AreEqual("Laptop Stand", overview.TopProductByQuantity.ProductName);
        Assert.AreEqual(2, overview.TopProductByQuantity.Quantity);
        Assert.AreEqual(100m, overview.TopProductByQuantity.Revenue);

        Assert.IsNotNull(overview.Comparison);
        Assert.AreEqual(25m, overview.Comparison.RevenueDeltaPct);
        Assert.AreEqual(300m, overview.Comparison.CostsDeltaPct);
        Assert.AreEqual(-250m, overview.Comparison.GainsDeltaPct);
        Assert.AreEqual(0m, overview.Comparison.SalesCountDeltaPct);
        Assert.AreEqual(100m, overview.Comparison.UnitsSoldDeltaPct);

        CollectionAssert.AreEquivalent(
            new[] { "Negative gains", "High debt exposure", "Products out of stock", "Low stock detected" },
            overview.Alerts.Select(alert => alert.Title).ToArray());
    }

    [TestMethod]
    public async Task GetOverviewAsync_WithEmptyPeriod_ShouldReturnZerosWithoutAlerts()
    {
        await using var db = CreateDbContext();
        var service = new DashboardService(db, new StubForecastService());

        var overview = await service.GetOverviewAsync(
            from: Utc(2026, 4, 1, 0, 0),
            to: Utc(2026, 4, 30, 23, 59));

        Assert.AreEqual(0m, overview.RevenueTotal);
        Assert.AreEqual(0m, overview.CostsTotal);
        Assert.AreEqual(0m, overview.Gains);
        Assert.AreEqual(0m, overview.MarginPct);
        Assert.AreEqual(0, overview.SalesCount);
        Assert.AreEqual(0, overview.UnitsSold);
        Assert.AreEqual(0m, overview.AvgTicket);
        Assert.AreEqual(0m, overview.DebtsTotal);
        Assert.AreEqual(0m, overview.DebtRatioPct);
        Assert.AreEqual(0, overview.LowStockCount);
        Assert.AreEqual(0, overview.OutOfStockCount);
        Assert.IsNull(overview.TopCustomer);
        Assert.IsNull(overview.TopProductByQuantity);
        Assert.AreEqual(0, overview.Alerts.Count);
    }

    [TestMethod]
    public async Task GetPerformanceSeriesAsync_ShouldBuildCurrentComparisonAndForecast()
    {
        await using var db = CreateDbContext();
        var product = new Product("Keyboard", 10m, initialStock: 20);
        db.Products.Add(product);

        db.Sales.AddRange(
            new Sale([new SaleItem(product.Id, 1, 10m)], createdAt: Utc(2026, 3, 1, 10, 0)),
            new Sale([new SaleItem(product.Id, 2, 10m)], createdAt: Utc(2026, 3, 2, 10, 0)),
            new Sale([new SaleItem(product.Id, 1, 7m)], createdAt: Utc(2025, 3, 1, 10, 0)),
            new Sale([new SaleItem(product.Id, 1, 12m)], createdAt: Utc(2025, 3, 2, 10, 0)),
            new Sale([new SaleItem(product.Id, 1, 5m)], createdAt: Utc(2024, 3, 1, 10, 0)),
            new Sale([new SaleItem(product.Id, 1, 9m)], createdAt: Utc(2024, 3, 2, 10, 0)),
            new Sale([new SaleItem(product.Id, 1, 3m)], createdAt: Utc(2023, 3, 1, 10, 0)));

        db.Costs.AddRange(
            new Cost("Infra", 4m, Utc(2026, 3, 1, 9, 0)),
            new Cost("Infra", 2m, Utc(2025, 3, 1, 9, 0)));
        await db.SaveChangesAsync();

        var forecastService = new StubForecastService
        {
            Result = new ForecastResult
            {
                ModelKey = "historical_average",
                ModelLabel = "Historical average",
                Priority = 200,
                UsedHistoricalComparisons = true,
                BasisYearsCount = 3,
                Series = new DashboardPerformanceSeriesLineDto
                {
                    Id = "forecast",
                    Label = "Forecast",
                    Kind = "forecast",
                    Points =
                    [
                        new DashboardPerformancePointDto
                        {
                            AxisIndex = 3,
                            AxisLabel = "03",
                            PeriodStart = Utc(2026, 3, 3, 0, 0),
                            Revenue = 15m
                        }
                    ]
                }
            }
        };

        var service = new DashboardService(db, forecastService);

        var result = await service.GetPerformanceSeriesAsync(
            groupBy: "DAY",
            from: Utc(2026, 3, 1, 0, 0),
            to: Utc(2026, 3, 3, 23, 59),
            tzOffsetMinutes: 0,
            compareYearOffsets: [1],
            forecastModel: "historical_average",
            includeForecast: true);

        Assert.AreEqual("day", result.GroupBy);
        Assert.AreEqual("day_of_period", result.AxisMode);
        Assert.AreEqual(3, result.CurrentSeries.Points.Count);
        Assert.AreEqual(10m, result.CurrentSeries.Points[0].Revenue);
        Assert.AreEqual(4m, result.CurrentSeries.Points[0].Costs);
        Assert.AreEqual(6m, result.CurrentSeries.Points[0].Gains);
        Assert.AreEqual(1, result.ComparisonSeries.Count);
        Assert.AreEqual("year-1", result.ComparisonSeries[0].Id);
        Assert.IsNotNull(result.ForecastSeries);
        Assert.AreEqual(15m, result.ForecastSeries.Points[0].Revenue);

        Assert.IsNotNull(forecastService.LastRequest);
        Assert.AreEqual("historical_average", forecastService.LastRequest.RequestedModelKey);
        Assert.AreEqual("day", forecastService.LastRequest.GroupBy);
        Assert.IsTrue(forecastService.LastRequest.HasFixedRange);
        Assert.AreEqual(3, forecastService.LastRequest.CurrentSeries.Points.Count);
        Assert.AreEqual(3, forecastService.LastRequest.YearComparisonSeries.Count);
    }

    [TestMethod]
    public async Task GetPerformanceSeriesAsync_WithComparisonsAndNoRange_ShouldThrow()
    {
        await using var db = CreateDbContext();
        var service = new DashboardService(db, new StubForecastService());

        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            service.GetPerformanceSeriesAsync(
                groupBy: "day",
                from: null,
                to: null,
                compareYearOffsets: [1],
                includeForecast: false));
    }

    [TestMethod]
    public async Task GetSalesByPeriodAsync_ShouldGroupRevenueAndSalesCountByDay()
    {
        await using var db = CreateDbContext();
        var product = new Product("Cable", 10m, initialStock: 10);
        db.Products.Add(product);

        db.Sales.AddRange(
            new Sale([new SaleItem(product.Id, 1, 10m)], createdAt: Utc(2026, 3, 1, 10, 0)),
            new Sale([new SaleItem(product.Id, 2, 10m)], createdAt: Utc(2026, 3, 1, 16, 0)),
            new Sale([new SaleItem(product.Id, 1, 25m)], createdAt: Utc(2026, 3, 2, 11, 0)),
            new Sale([new SaleItem(product.Id, 1, 99m)], isDebt: true, createdAt: Utc(2026, 3, 2, 12, 0)));
        await db.SaveChangesAsync();

        var service = new DashboardService(db, new StubForecastService());

        var result = await service.GetSalesByPeriodAsync(
            groupBy: "day",
            from: Utc(2026, 3, 1, 0, 0),
            to: Utc(2026, 3, 2, 23, 59));

        Assert.AreEqual("day", result.GroupBy);
        Assert.AreEqual(2, result.Points.Count);
        Assert.AreEqual(Utc(2026, 3, 1, 0, 0), result.Points[0].PeriodStart);
        Assert.AreEqual(2, result.Points[0].SalesCount);
        Assert.AreEqual(30m, result.Points[0].Revenue);
        Assert.AreEqual(Utc(2026, 3, 2, 0, 0), result.Points[1].PeriodStart);
        Assert.AreEqual(1, result.Points[1].SalesCount);
        Assert.AreEqual(25m, result.Points[1].Revenue);
    }

    [TestMethod]
    public async Task GetTopProductsAsync_ShouldRespectLimitSortAndExcludeDebts()
    {
        await using var db = CreateDbContext();
        var alpha = new Product("Alpha", 10m, initialStock: 10);
        var beta = new Product("Beta", 20m, initialStock: 10);
        var gamma = new Product("Gamma", 5m, initialStock: 10);
        db.Products.AddRange(alpha, beta, gamma);

        db.Sales.AddRange(
            new Sale([new SaleItem(alpha.Id, 2, 10m), new SaleItem(beta.Id, 1, 20m)], createdAt: Utc(2026, 3, 1, 10, 0)),
            new Sale([new SaleItem(alpha.Id, 1, 10m), new SaleItem(gamma.Id, 5, 5m)], createdAt: Utc(2026, 3, 2, 10, 0)),
            new Sale([new SaleItem(beta.Id, 10, 20m)], isDebt: true, createdAt: Utc(2026, 3, 2, 12, 0)));
        await db.SaveChangesAsync();

        var service = new DashboardService(db, new StubForecastService());

        var result = await service.GetTopProductsAsync(
            limit: 2,
            from: Utc(2026, 3, 1, 0, 0),
            to: Utc(2026, 3, 2, 23, 59),
            sortBy: "quantity");

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Gamma", result[0].ProductName);
        Assert.AreEqual(5, result[0].Quantity);
        Assert.AreEqual(25m, result[0].Revenue);
        Assert.AreEqual("Alpha", result[1].ProductName);
        Assert.AreEqual(3, result[1].Quantity);
        Assert.AreEqual(30m, result[1].Revenue);
    }

    [TestMethod]
    public async Task GetSalesByCustomerAsync_ShouldAggregateAndOptionallyExcludeDebts()
    {
        await using var db = CreateDbContext();
        var product = new Product("Speaker", 30m, initialStock: 10);
        var alice = new Customer("Alice");
        var bob = new Customer("Bob");
        db.Products.Add(product);
        db.Customers.AddRange(alice, bob);

        var aliceSale1 = new Sale([new SaleItem(product.Id, 1, 30m)], customerId: alice.Id, createdAt: Utc(2026, 3, 1, 10, 0));
        aliceSale1.SetCustomer(alice);
        var aliceSale2 = new Sale([new SaleItem(product.Id, 1, 30m)], customerId: alice.Id, isDebt: true, createdAt: Utc(2026, 3, 1, 12, 0));
        aliceSale2.SetCustomer(alice);
        var bobSale = new Sale([new SaleItem(product.Id, 1, 30m)], customerId: bob.Id, createdAt: Utc(2026, 3, 1, 14, 0));
        bobSale.SetCustomer(bob);

        db.Sales.AddRange(aliceSale1, aliceSale2, bobSale);
        await db.SaveChangesAsync();

        var service = new DashboardService(db, new StubForecastService());

        var excludingDebts = await service.GetSalesByCustomerAsync(
            limit: 10,
            from: Utc(2026, 3, 1, 0, 0),
            to: Utc(2026, 3, 1, 23, 59),
            excludeDebts: true);

        Assert.AreEqual(2, excludingDebts.Count);
        Assert.AreEqual("Alice", excludingDebts[0].CustomerName);
        Assert.AreEqual(1, excludingDebts[0].SalesCount);
        Assert.AreEqual("Bob", excludingDebts[1].CustomerName);
        Assert.AreEqual(1, excludingDebts[1].SalesCount);

        var includingDebts = await service.GetSalesByCustomerAsync(
            limit: 10,
            from: Utc(2026, 3, 1, 0, 0),
            to: Utc(2026, 3, 1, 23, 59),
            excludeDebts: false);

        Assert.AreEqual("Alice", includingDebts[0].CustomerName);
        Assert.AreEqual(2, includingDebts[0].SalesCount);
    }

    [TestMethod]
    public async Task GetSpendingByCustomerAsync_ShouldAggregateRevenueAndExcludeDebtsByDefault()
    {
        await using var db = CreateDbContext();
        var product = new Product("Phone", 50m, initialStock: 10);
        var alice = new Customer("Alice");
        var bob = new Customer("Bob");
        db.Products.Add(product);
        db.Customers.AddRange(alice, bob);

        var aliceSale = new Sale([new SaleItem(product.Id, 2, 50m)], customerId: alice.Id, createdAt: Utc(2026, 3, 3, 10, 0));
        aliceSale.SetCustomer(alice);
        var bobSale = new Sale([new SaleItem(product.Id, 1, 80m)], customerId: bob.Id, createdAt: Utc(2026, 3, 3, 11, 0));
        bobSale.SetCustomer(bob);
        var debtSale = new Sale([new SaleItem(product.Id, 1, 200m)], customerId: bob.Id, isDebt: true, createdAt: Utc(2026, 3, 3, 12, 0));
        debtSale.SetCustomer(bob);

        db.Sales.AddRange(aliceSale, bobSale, debtSale);
        await db.SaveChangesAsync();

        var service = new DashboardService(db, new StubForecastService());

        var excludingDebts = await service.GetSpendingByCustomerAsync(
            limit: 10,
            from: Utc(2026, 3, 3, 0, 0),
            to: Utc(2026, 3, 3, 23, 59));

        Assert.AreEqual(2, excludingDebts.Count);
        Assert.AreEqual("Alice", excludingDebts[0].CustomerName);
        Assert.AreEqual(100m, excludingDebts[0].TotalSpent);
        Assert.AreEqual("Bob", excludingDebts[1].CustomerName);
        Assert.AreEqual(80m, excludingDebts[1].TotalSpent);

        var includingDebts = await service.GetSpendingByCustomerAsync(
            limit: 10,
            from: Utc(2026, 3, 3, 0, 0),
            to: Utc(2026, 3, 3, 23, 59),
            excludeDebts: false);

        Assert.AreEqual("Bob", includingDebts[0].CustomerName);
        Assert.AreEqual(280m, includingDebts[0].TotalSpent);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AppDbContext(options);
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute)
        => new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    private sealed class StubForecastService : IForecastService
    {
        public ForecastRequest? LastRequest { get; private set; }
        public ForecastResult? Result { get; init; }

        public ForecastResult? BuildForecast(ForecastRequest request)
        {
            LastRequest = request;
            return Result;
        }
    }
}
