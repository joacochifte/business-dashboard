using BusinessDashboard.Application.Promotions;
using BusinessDashboard.Infrastructure.Promotions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class PromotionsControllerTests
{
    private FakePromotionRecommendationService _service = null!;
    private PromotionsController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakePromotionRecommendationService();
        _controller = new PromotionsController(_service);
    }

    [TestMethod]
    public async Task GetRecommendations_ShouldReturnOkWithRecommendations()
    {
        _service.Result =
        [
            new PromotionRecommendationDto
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "Alice",
                Score = 88m,
                Reason = "recent buyer"
            },
            new PromotionRecommendationDto
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "Bob",
                Score = 62m,
                Reason = "frequent in last 90 days"
            }
        ];

        var result = await _controller.GetRecommendations();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<PromotionRecommendationDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count());
    }

    [TestMethod]
    public async Task GetRecommendations_ShouldForwardLimitToService()
    {
        await _controller.GetRecommendations(limit: 25, cancellationToken: CancellationToken.None);

        Assert.AreEqual(25, _service.LastLimit);
    }

    private sealed class FakePromotionRecommendationService : IPromotionRecommendationService
    {
        public IReadOnlyList<PromotionRecommendationDto> Result { get; set; } = Array.Empty<PromotionRecommendationDto>();
        public int? LastLimit { get; private set; }

        public Task<IReadOnlyList<PromotionRecommendationDto>> GetRecommendationsAsync(int limit = 10, CancellationToken ct = default)
        {
            LastLimit = limit;
            return Task.FromResult(Result);
        }
    }
}
