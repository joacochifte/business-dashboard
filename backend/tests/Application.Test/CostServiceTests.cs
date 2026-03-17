using BusinessDashboard.Application.Costs;
using BusinessDashboard.Domain.Common.Exceptions;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Infrastructure.Costs;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Moq;

namespace Application.Test.Costs;

[TestClass]
public class CostServiceTests
{
    private Mock<ICostRepository> _repo = null!;
    private CostService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repo = new Mock<ICostRepository>(MockBehavior.Strict);
        _service = new CostService(_repo.Object);
    }

    [TestMethod]
    public async Task AddCostAsync_ShouldCreateAndReturnId()
    {
        var request = new CostCreationDto
        {
            Name = "Rent",
            Amount = 1200m,
            DateIncurred = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            Description = "Monthly office rent"
        };

        Cost? captured = null;
        _repo.Setup(r => r.AddCostAsync(It.IsAny<Cost>(), It.IsAny<CancellationToken>()))
            .Callback<Cost, CancellationToken>((cost, _) => captured = cost)
            .Returns(Task.CompletedTask);

        var id = await _service.AddCostAsync(request);

        Assert.IsNotNull(captured);
        Assert.AreEqual(request.Name, captured.Name);
        Assert.AreEqual(request.Amount, captured.Amount);
        Assert.AreEqual(request.DateIncurred, captured.DateIncurred);
        Assert.AreEqual(request.Description, captured.Description);
        Assert.AreEqual(captured.Id, id);

        _repo.Verify(r => r.AddCostAsync(It.IsAny<Cost>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetCostsAsync_ShouldMapToSummaryDtos()
    {
        var costs = new List<Cost>
        {
            new Cost("Rent", 1000m, new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            new Cost("Utilities", 250m, new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc))
        };

        _repo.Setup(r => r.GetCostsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(costs);

        var result = (await _service.GetCostsAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Rent", result[0].Name);
        Assert.AreEqual(1000m, result[0].Amount);
        Assert.AreEqual("Utilities", result[1].Name);
        Assert.AreEqual(250m, result[1].Amount);

        _repo.Verify(r => r.GetCostsAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetCostByIdAsync_ShouldReturnRepositoryResult()
    {
        var id = Guid.NewGuid();
        var cost = new Cost("Marketing", 500m, new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc));

        _repo.Setup(r => r.GetCostByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cost);

        var result = await _service.GetCostByIdAsync(id);

        Assert.IsNotNull(result);
        Assert.AreEqual("Marketing", result.Name);
        Assert.AreEqual(500m, result.Amount);

        _repo.Verify(r => r.GetCostByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateCostAsync_ShouldUpdateAndPersist()
    {
        var id = Guid.NewGuid();
        var existing = new Cost("Old name", 100m, new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), "Old description");
        var request = new CostCreationDto
        {
            Name = "New name",
            Amount = 250m,
            DateIncurred = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc),
            Description = "New description"
        };

        _repo.Setup(r => r.GetCostByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateCostAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.UpdateCostAsync(id, request);

        Assert.AreEqual("New name", existing.Name);
        Assert.AreEqual(250m, existing.Amount);
        Assert.AreEqual(request.DateIncurred, existing.DateIncurred);
        Assert.AreEqual("New description", existing.Description);

        _repo.Verify(r => r.UpdateCostAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    public async Task UpdateCostAsync_WhenCostDoesNotExist_ShouldThrow()
    {
        var id = Guid.NewGuid();
        var request = new CostCreationDto
        {
            Name = "Missing",
            Amount = 1m,
            DateIncurred = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        _repo.Setup(r => r.GetCostByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cost?)null);

        await _service.UpdateCostAsync(id, request);
    }

    [TestMethod]
    public async Task DeleteCostAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.DeleteCostAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteCostAsync(id);

        _repo.Verify(r => r.DeleteCostAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
