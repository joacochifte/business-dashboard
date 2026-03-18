using BusinessDashboard.Application.Promotions.Scoring;
using BusinessDashboard.Infrastructure.Promotions;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

namespace BusinessDashboard.Application.Promotions;

public sealed class PromotionRecommendationService : IPromotionRecommendationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IPromotionScoringStrategy _scoringStrategy;
    private readonly TimeProvider _timeProvider;

    public PromotionRecommendationService(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IPromotionScoringStrategy scoringStrategy,
        TimeProvider timeProvider)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _scoringStrategy = scoringStrategy;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<PromotionRecommendationDto>> GetRecommendationsAsync(int limit = 10, CancellationToken ct = default)
    {
        var effectiveLimit = Math.Clamp(limit, 1, 100);
        var customers = (await _customerRepository.GetAllAsync(ct)).ToList();
        if (customers.Count == 0)
            return Array.Empty<PromotionRecommendationDto>();

        var sales = (await _saleRepository.GetAllAsync()).ToList();
        var productsById = (await _productRepository.GetAllAsync())
            .ToDictionary(product => product.Id);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var recentThreshold = now.AddDays(-90);

        var recommendations = customers
            .Select(customer =>
            {
                var customerSales = sales
                    .Where(s => s.CustomerId == customer.Id)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToList();

                if (customerSales.Count == 0)
                    return null;

                var totalRevenue = customerSales.Sum(s => s.Total);
                var debtRevenue = customerSales.Where(s => s.IsDebt).Sum(s => s.Total);
                var metrics = new PromotionCandidateMetrics
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.Name,
                    DaysSinceLastPurchase = Math.Max(0, (now.Date - customerSales[0].CreatedAt.Date).Days),
                    PurchasesLast90Days = customerSales.Count(s => s.CreatedAt >= recentThreshold),
                    AvgTicket = Math.Round(customerSales.Average(s => s.Total), 2),
                    DebtRatioPct = totalRevenue == 0m ? 0m : Math.Round((debtRevenue / totalRevenue) * 100m, 2),
                    TotalPurchases = customerSales.Count
                };

                var score = _scoringStrategy.Score(metrics);
                var productRecommendation = BuildProductRecommendation(customerSales, productsById, now);

                return new PromotionRecommendationDto
                {
                    CustomerId = metrics.CustomerId,
                    CustomerName = metrics.CustomerName,
                    Score = score.Score,
                    Reason = score.Reason,
                    RecommendedProductId = productRecommendation?.ProductId,
                    RecommendedProductName = productRecommendation?.ProductName,
                    ProductRecommendationReason = productRecommendation?.Reason,
                    DaysSinceLastPurchase = metrics.DaysSinceLastPurchase,
                    PurchasesLast90Days = metrics.PurchasesLast90Days,
                    AvgTicket = metrics.AvgTicket,
                    DebtRatioPct = metrics.DebtRatioPct,
                    TotalPurchases = metrics.TotalPurchases
                };
            })
            .Where(recommendation => recommendation is not null)
            .Select(recommendation => recommendation!)
            .OrderByDescending(recommendation => recommendation.Score)
            .ThenBy(recommendation => recommendation.CustomerName)
            .Take(effectiveLimit)
            .ToList();

        return recommendations;
    }

    private static ProductPromotionRecommendation? BuildProductRecommendation(
        IReadOnlyList<Domain.Sales.Sale> customerSales,
        IReadOnlyDictionary<Guid, Domain.Products.Product> productsById,
        DateTime now)
    {
        var candidates = customerSales
            .SelectMany(
                sale => sale.Items,
                (sale, item) => new
                {
                    sale.CreatedAt,
                    item.ProductId,
                    item.Quantity,
                    Revenue = item.LineTotal
                })
            .GroupBy(x => x.ProductId)
            .Select(group =>
            {
                if (!productsById.TryGetValue(group.Key, out var product) || !product.IsActive)
                    return null;

                var purchaseCount = group.Count();
                var totalQuantity = group.Sum(x => x.Quantity);
                var totalRevenue = group.Sum(x => x.Revenue);
                var lastPurchasedAt = group.Max(x => x.CreatedAt);
                var daysSinceLastPurchase = Math.Max(0, (now.Date - lastPurchasedAt.Date).Days);

                var score = BuildProductRecommendationScore(
                    purchaseCount,
                    totalQuantity,
                    totalRevenue,
                    daysSinceLastPurchase);

                return new ProductPromotionRecommendation
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Reason = BuildProductRecommendationReason(product.Name, purchaseCount, daysSinceLastPurchase),
                    Score = score,
                    IsAvailable = product.Stock is null || product.Stock > 0
                };
            })
            .Where(candidate => candidate is not null)
            .Select(candidate => candidate!)
            .ToList();

        return candidates
            .OrderByDescending(candidate => candidate.IsAvailable)
            .ThenByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.ProductName)
            .FirstOrDefault();
    }

    private static decimal BuildProductRecommendationScore(
        int purchaseCount,
        int totalQuantity,
        decimal totalRevenue,
        int daysSinceLastPurchase)
    {
        var recencyWindowScore = daysSinceLastPurchase switch
        {
            <= 7 => 0m,
            <= 30 => 18m,
            <= 90 => 28m,
            <= 180 => 16m,
            _ => 8m,
        };

        var purchaseCountScore = Math.Min(purchaseCount * 10m, 30m);
        var quantityScore = Math.Min(totalQuantity * 3m, 20m);
        var revenueScore = Math.Min(totalRevenue / 25m, 20m);

        return recencyWindowScore + purchaseCountScore + quantityScore + revenueScore;
    }

    private static string BuildProductRecommendationReason(string productName, int purchaseCount, int daysSinceLastPurchase)
    {
        if (daysSinceLastPurchase >= 30)
            return $"Bought {purchaseCount} time(s) before, last purchased {daysSinceLastPurchase} day(s) ago.";

        return $"{productName} is one of this customer's strongest historical products.";
    }

    private sealed class ProductPromotionRecommendation
    {
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public decimal Score { get; init; }
        public bool IsAvailable { get; init; }
    }
}
