using BusinessDashboard.Infrastructure.Promotions;

namespace BusinessDashboard.Application.Promotions;

public interface IPromotionRecommendationService
{
    Task<IReadOnlyList<PromotionRecommendationDto>> GetRecommendationsAsync(int limit = 10, CancellationToken ct = default);
}
