using BusinessDashboard.Application.Promotions;
using BusinessDashboard.Application.Promotions.Scoring;
using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Moq;

namespace Application.Test.Promotions;

[TestClass]
public class PromotionRecommendationServiceTests
{
    [TestMethod]
    public void RfmPromotionScoringStrategy_ShouldScoreRecentFrequentHealthyCustomerHigher()
    {
        var strategy = new RfmPromotionScoringStrategy();

        var strong = strategy.Score(new PromotionCandidateMetrics
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Strong",
            DaysSinceLastPurchase = 5,
            PurchasesLast90Days = 6,
            AvgTicket = 140m,
            DebtRatioPct = 0m,
            TotalPurchases = 12
        });

        var weak = strategy.Score(new PromotionCandidateMetrics
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Weak",
            DaysSinceLastPurchase = 140,
            PurchasesLast90Days = 1,
            AvgTicket = 25m,
            DebtRatioPct = 0m,
            TotalPurchases = 1
        });

        Assert.IsTrue(strong.Score > weak.Score);
        StringAssert.Contains(strong.Reason, "recent buyer");
        StringAssert.Contains(strong.Reason, "frequent in last 90 days");
    }

    [TestMethod]
    public void RfmPromotionScoringStrategy_ShouldPenalizeHighDebtProfile()
    {
        var strategy = new RfmPromotionScoringStrategy();

        var healthy = strategy.Score(new PromotionCandidateMetrics
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Healthy",
            DaysSinceLastPurchase = 10,
            PurchasesLast90Days = 4,
            AvgTicket = 120m,
            DebtRatioPct = 0m,
            TotalPurchases = 8
        });

        var risky = strategy.Score(new PromotionCandidateMetrics
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Risky",
            DaysSinceLastPurchase = 10,
            PurchasesLast90Days = 4,
            AvgTicket = 120m,
            DebtRatioPct = 75m,
            TotalPurchases = 8
        });

        Assert.IsTrue(healthy.Score > risky.Score);
        StringAssert.Contains(risky.Reason, "debt profile penalized");
    }

    [TestMethod]
    public async Task GetRecommendationsAsync_ShouldReturnCustomersOrderedByScore()
    {
        var customers = new[]
        {
            new Customer("Alice"),
            new Customer("Bob"),
            new Customer("No Sales")
        };

        var now = new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero);
        var sales = new List<Sale>
        {
            CreateSale(customers[0], 120m, false, now.AddDays(-5).UtcDateTime),
            CreateSale(customers[0], 110m, false, now.AddDays(-20).UtcDateTime),
            CreateSale(customers[0], 90m, false, now.AddDays(-40).UtcDateTime),
            CreateSale(customers[1], 150m, true, now.AddDays(-6).UtcDateTime),
            CreateSale(customers[1], 80m, false, now.AddDays(-12).UtcDateTime),
        };

        var customerRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var saleRepo = new Mock<ISaleRepository>(MockBehavior.Strict);
        customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(customers);
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

        var service = new PromotionRecommendationService(
            customerRepo.Object,
            saleRepo.Object,
            new RfmPromotionScoringStrategy(),
            new FakeTimeProvider(now));

        var result = await service.GetRecommendationsAsync(limit: 10);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Alice", result[0].CustomerName);
        Assert.AreEqual("Bob", result[1].CustomerName);
        Assert.IsTrue(result[0].Score > result[1].Score);
        Assert.AreEqual(3, result[0].PurchasesLast90Days);
        Assert.AreEqual(0m, result[0].DebtRatioPct);
        Assert.AreEqual(2, result[1].PurchasesLast90Days);
        Assert.IsTrue(result[1].DebtRatioPct > 40m);
    }

    [TestMethod]
    public async Task GetRecommendationsAsync_ShouldRespectLimit()
    {
        var customers = new[]
        {
            new Customer("Alice"),
            new Customer("Bob"),
            new Customer("Charlie"),
        };

        var now = new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero);
        var sales = new List<Sale>
        {
            CreateSale(customers[0], 100m, false, now.AddDays(-3).UtcDateTime),
            CreateSale(customers[1], 90m, false, now.AddDays(-8).UtcDateTime),
            CreateSale(customers[2], 80m, false, now.AddDays(-30).UtcDateTime),
        };

        var customerRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var saleRepo = new Mock<ISaleRepository>(MockBehavior.Strict);
        customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(customers);
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

        var service = new PromotionRecommendationService(
            customerRepo.Object,
            saleRepo.Object,
            new RfmPromotionScoringStrategy(),
            new FakeTimeProvider(now));

        var result = await service.GetRecommendationsAsync(limit: 2);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetRecommendationsAsync_WithNoEligibleCustomers_ShouldReturnEmpty()
    {
        var customerRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var saleRepo = new Mock<ISaleRepository>(MockBehavior.Strict);
        customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { new Customer("Alice") });
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Sale>());

        var service = new PromotionRecommendationService(
            customerRepo.Object,
            saleRepo.Object,
            new RfmPromotionScoringStrategy(),
            new FakeTimeProvider(new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetRecommendationsAsync();

        Assert.AreEqual(0, result.Count);
    }

    private static Sale CreateSale(Customer customer, decimal total, bool isDebt, DateTime createdAt)
    {
        var sale = new Sale(
            new[] { new SaleItem(Guid.NewGuid(), 1, total) },
            customerId: customer.Id,
            isDebt: isDebt,
            createdAt: createdAt);
        sale.SetCustomer(customer);
        return sale;
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FakeTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
