namespace BusinessDashboard.Application.Promotions.Scoring;

public sealed class RfmPromotionScoringStrategy : IPromotionScoringStrategy
{
    public PromotionScoreResult Score(PromotionCandidateMetrics candidate)
    {
        var recencyScore = candidate.DaysSinceLastPurchase switch
        {
            <= 7 => 45m,
            <= 30 => 35m,
            <= 60 => 20m,
            <= 120 => 8m,
            _ => 0m,
        };

        var frequencyScore = candidate.PurchasesLast90Days switch
        {
            >= 6 => 30m,
            >= 3 => 20m,
            >= 1 => 10m,
            _ => 0m,
        };

        var monetaryScore = candidate.AvgTicket switch
        {
            >= 200m => 15m,
            >= 100m => 10m,
            >= 50m => 6m,
            > 0m => 2m,
            _ => 0m,
        };

        var loyaltyScore = candidate.TotalPurchases switch
        {
            >= 12 => 10m,
            >= 6 => 6m,
            >= 3 => 3m,
            _ => 0m,
        };

        var debtPenalty = candidate.DebtRatioPct switch
        {
            >= 70m => 25m,
            >= 40m => 15m,
            > 0m => 6m,
            _ => 0m,
        };

        var rawScore = recencyScore + frequencyScore + monetaryScore + loyaltyScore - debtPenalty;
        var score = Math.Round(Math.Clamp(rawScore, 0m, 100m), 2);

        var reasons = new List<string>(4);
        if (candidate.DaysSinceLastPurchase <= 30) reasons.Add("recent buyer");
        if (candidate.PurchasesLast90Days >= 3) reasons.Add("frequent in last 90 days");
        if (candidate.AvgTicket >= 100m) reasons.Add("strong average ticket");
        if (candidate.DebtRatioPct >= 40m) reasons.Add("debt profile penalized");

        if (reasons.Count == 0)
            reasons.Add("low recent activity");

        return new PromotionScoreResult
        {
            Score = score,
            Reason = string.Join(", ", reasons)
        };
    }
}
