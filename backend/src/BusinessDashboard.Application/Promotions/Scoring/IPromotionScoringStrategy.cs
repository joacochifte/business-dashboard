namespace BusinessDashboard.Application.Promotions.Scoring;

public interface IPromotionScoringStrategy
{
    PromotionScoreResult Score(PromotionCandidateMetrics candidate);
}
