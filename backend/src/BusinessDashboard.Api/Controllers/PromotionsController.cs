using BusinessDashboard.Application.Promotions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("promotions")]
public class PromotionsController(IPromotionRecommendationService promotionRecommendationService) : ControllerBase
{
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var recommendations = await promotionRecommendationService.GetRecommendationsAsync(limit, cancellationToken);
        return Ok(recommendations);
    }
}
