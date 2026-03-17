using BusinessDashboard.Application.Promotions.Scoring;
using BusinessDashboard.Infrastructure.Promotions;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

namespace BusinessDashboard.Application.Promotions;

public sealed class PromotionRecommendationService : IPromotionRecommendationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IPromotionScoringStrategy _scoringStrategy;
    private readonly TimeProvider _timeProvider;

    public PromotionRecommendationService(
        ICustomerRepository customerRepository,
        ISaleRepository saleRepository,
        IPromotionScoringStrategy scoringStrategy,
        TimeProvider timeProvider)
    {
        _customerRepository = customerRepository;
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
                return new PromotionRecommendationDto
                {
                    CustomerId = metrics.CustomerId,
                    CustomerName = metrics.CustomerName,
                    Score = score.Score,
                    Reason = score.Reason,
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
}
