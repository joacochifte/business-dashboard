namespace BusinessDashboard.Application.Promotions.Scoring;

public sealed class PromotionCandidateMetrics
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int DaysSinceLastPurchase { get; init; }
    public int PurchasesLast90Days { get; init; }
    public decimal AvgTicket { get; init; }
    public decimal DebtRatioPct { get; init; }
    public int TotalPurchases { get; init; }
}
