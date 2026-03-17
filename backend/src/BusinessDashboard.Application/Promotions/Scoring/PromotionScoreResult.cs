namespace BusinessDashboard.Application.Promotions.Scoring;

public sealed class PromotionScoreResult
{
    public decimal Score { get; init; }
    public string Reason { get; init; } = string.Empty;
}
