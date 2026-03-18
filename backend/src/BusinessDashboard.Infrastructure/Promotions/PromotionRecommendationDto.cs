namespace BusinessDashboard.Infrastructure.Promotions;

public sealed class PromotionRecommendationDto
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal Score { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid? RecommendedProductId { get; init; }
    public string? RecommendedProductName { get; init; }
    public string? ProductRecommendationReason { get; init; }
    public int DaysSinceLastPurchase { get; init; }
    public int PurchasesLast90Days { get; init; }
    public decimal AvgTicket { get; init; }
    public decimal DebtRatioPct { get; init; }
    public int TotalPurchases { get; init; }
}
