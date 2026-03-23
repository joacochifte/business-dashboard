using BusinessDashboard.Application.Promotions;
using BusinessDashboard.Application.Promotions.Scoring;
using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Domain.Products;
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
        var proteinBar = new Product("Protein Bar", 20m, 25);
        var energyDrink = new Product("Energy Drink", 12m, 40);
        var sales = new List<Sale>
        {
            CreateSale(customers[0], false, now.AddDays(-5).UtcDateTime, (proteinBar.Id, 2, 60m)),
            CreateSale(customers[0], false, now.AddDays(-20).UtcDateTime, (proteinBar.Id, 1, 110m)),
            CreateSale(customers[0], false, now.AddDays(-40).UtcDateTime, (energyDrink.Id, 1, 90m)),
            CreateSale(customers[1], true, now.AddDays(-6).UtcDateTime, (energyDrink.Id, 1, 150m)),
            CreateSale(customers[1], false, now.AddDays(-12).UtcDateTime, (energyDrink.Id, 1, 80m)),
        };

        var customerRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var productRepo = new Mock<IProductRepository>(MockBehavior.Strict);
        var saleRepo = new Mock<ISaleRepository>(MockBehavior.Strict);
        customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(customers);
        productRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { proteinBar, energyDrink });
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

        var service = new PromotionRecommendationService(
            customerRepo.Object,
            productRepo.Object,
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
        Assert.AreEqual(energyDrink.Id, result[0].RecommendedProductId);
        Assert.AreEqual("Energy Drink", result[0].RecommendedProductName);
        Assert.AreEqual(2, result[1].PurchasesLast90Days);
        Assert.IsTrue(result[1].DebtRatioPct > 40m);
        Assert.AreEqual(energyDrink.Id, result[1].RecommendedProductId);
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
        var productA = new Product("A", 10m, 10);
        var productB = new Product("B", 10m, 10);
        var productC = new Product("C", 10m, 10);
        var sales = new List<Sale>
        {
            CreateSale(customers[0], false, now.AddDays(-3).UtcDateTime, (productA.Id, 1, 100m)),
            CreateSale(customers[1], false, now.AddDays(-8).UtcDateTime, (productB.Id, 1, 90m)),
            CreateSale(customers[2], false, now.AddDays(-30).UtcDateTime, (productC.Id, 1, 80m)),
        };

        var customerRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var productRepo = new Mock<IProductRepository>(MockBehavior.Strict);
        var saleRepo = new Mock<ISaleRepository>(MockBehavior.Strict);
        customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(customers);
        productRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { productA, productB, productC });
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

        var service = new PromotionRecommendationService(
            customerRepo.Object,
            productRepo.Object,
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
        var productRepo = new Mock<IProductRepository>(MockBehavior.Strict);
        var saleRepo = new Mock<ISaleRepository>(MockBehavior.Strict);
        customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { new Customer("Alice") });
        productRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Sale>());

        var service = new PromotionRecommendationService(
            customerRepo.Object,
            productRepo.Object,
            saleRepo.Object,
            new RfmPromotionScoringStrategy(),
            new FakeTimeProvider(new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetRecommendationsAsync();

        Assert.AreEqual(0, result.Count);
    }

    private static Sale CreateSale(
        Customer customer,
        bool isDebt,
        DateTime createdAt,
        params (Guid ProductId, int Quantity, decimal UnitPrice)[] items)
    {
        var sale = new Sale(
            items.Select(item => new SaleItem(item.ProductId, item.Quantity, item.UnitPrice)),
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
