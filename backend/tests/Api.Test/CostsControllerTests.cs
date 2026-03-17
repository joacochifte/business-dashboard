using BusinessDashboard.Application.Costs;
using BusinessDashboard.Api.Controllers;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Infrastructure.Costs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class CostsControllerTests
{
    private FakeCostService _service = null!;
    private CostsController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeCostService();
        _controller = new CostsController(_service);
    }

    [TestMethod]
    public async Task AddCost_ShouldReturnCreatedAtAction()
    {
        var id = Guid.NewGuid();
        _service.AddResult = id;
        var request = new CostCreationDto
        {
            Name = "Rent",
            Amount = 1000m,
            DateIncurred = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _controller.AddCost(request, CancellationToken.None);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(nameof(CostsController.GetCostById), created.ActionName);
        Assert.IsNotNull(created.RouteValues);
        Assert.AreEqual(id, created.RouteValues["id"]);
    }

    [TestMethod]
    public async Task GetCosts_ShouldReturnOkWithCosts()
    {
        _service.GetCostsResult =
        [
            new CostSummaryDto { Id = Guid.NewGuid(), Name = "Rent", Amount = 1000m, DateIncurred = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CostSummaryDto { Id = Guid.NewGuid(), Name = "Ads", Amount = 200m, DateIncurred = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc) }
        ];

        var result = await _controller.GetCosts(null, null, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<CostSummaryDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count());
    }

    [TestMethod]
    public async Task GetCosts_WithInvalidRange_ShouldReturnBadRequest()
    {
        var result = await _controller.GetCosts(
            new DateTime(2026, 3, 10),
            new DateTime(2026, 3, 1),
            CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetCostById_ShouldReturnOkWithDto()
    {
        var id = Guid.NewGuid();
        _service.GetByIdResult = new Cost("Rent", 1000m, new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), "Monthly rent");

        var result = await _controller.GetCostById(id, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var dto = ok.Value as CostCreationDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual("Rent", dto.Name);
        Assert.AreEqual(1000m, dto.Amount);
        Assert.AreEqual("Monthly rent", dto.Description);
    }

    [TestMethod]
    public async Task GetCostById_WhenMissing_ShouldReturnNotFound()
    {
        _service.GetByIdResult = null;

        var result = await _controller.GetCostById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task UpdateCost_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();
        var request = new CostCreationDto
        {
            Name = "Updated",
            Amount = 250m,
            DateIncurred = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _controller.UpdateCost(id, request, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        Assert.AreEqual(id, _service.LastUpdatedId);
    }

    [TestMethod]
    public async Task DeleteCost_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();

        var result = await _controller.DeleteCost(id, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        Assert.AreEqual(id, _service.LastDeletedId);
    }

    private sealed class FakeCostService : ICostService
    {
        public Guid AddResult { get; set; } = Guid.NewGuid();
        public IEnumerable<CostSummaryDto> GetCostsResult { get; set; } = Array.Empty<CostSummaryDto>();
        public Cost? GetByIdResult { get; set; }
        public Guid? LastUpdatedId { get; private set; }
        public Guid? LastDeletedId { get; private set; }

        public Task<Guid> AddCostAsync(CostCreationDto costDto, CancellationToken cancellationToken = default)
            => Task.FromResult(AddResult);

        public Task DeleteCostAsync(Guid id, CancellationToken cancellationToken = default)
        {
            LastDeletedId = id;
            return Task.CompletedTask;
        }

        public Task<Cost?> GetCostByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(GetByIdResult);

        public Task<IEnumerable<CostSummaryDto>> GetCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
            => Task.FromResult(GetCostsResult);

        public Task UpdateCostAsync(Guid id, CostCreationDto costDto, CancellationToken cancellationToken = default)
        {
            LastUpdatedId = id;
            return Task.CompletedTask;
        }
    }
}
