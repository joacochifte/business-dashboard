using BusinessDashboard.Application.Inventory;
using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Infrastructure.Inventory;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class InventoryControllerTests
{
    private FakeInventoryService _service = null!;
    private FakeInventoryRepository _repo = null!;
    private InventoryController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeInventoryService();
        _repo = new FakeInventoryRepository();
        _controller = new InventoryController(_service, _repo);
    }

    [TestMethod]
    public async Task Adjust_ShouldReturnOk_AndCallService()
    {
        var productId = Guid.NewGuid();
        var request = new InventoryMovementDto { ProductId = productId, Delta = 5 };

        var result = await _controller.Adjust(request, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(productId, _service.LastProductId);
        Assert.AreEqual(5, _service.LastDelta);
    }

    [TestMethod]
    public async Task GetMovements_WithNoFilters_ShouldReturnOkWithPage()
    {
        var productId = Guid.NewGuid();
        var m1 = new InventoryMovement(productId, InventoryMovementType.In, InventoryMovementReason.Purchase, 10);
        var m2 = new InventoryMovement(productId, InventoryMovementType.Out, InventoryMovementReason.Sale, 2);

        _repo.PageItems = new List<InventoryMovement> { m1, m2 };
        _repo.PageTotal = 2;

        var result = await _controller.GetMovements(
            CancellationToken.None,
            productId: null,
            from: null,
            to: null,
            page: 1,
            pageSize: 50);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);

        var page = ok.Value as InventoryMovementsPageDto;
        Assert.IsNotNull(page);

        Assert.AreEqual(1, page.Page);
        Assert.AreEqual(50, page.PageSize);
        Assert.AreEqual(2, page.Total);
        Assert.AreEqual(2, page.Items.Count);

        Assert.AreEqual(productId, page.Items[0].ProductId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(page.Items[0].Type));
        Assert.IsFalse(string.IsNullOrWhiteSpace(page.Items[0].Reason));
    }

    [TestMethod]
    public async Task GetMovements_WithEmptyProductId_ShouldReturnBadRequest()
    {
        var result = await _controller.GetMovements(
            CancellationToken.None,
            productId: Guid.Empty,
            from: null,
            to: null,
            page: 1,
            pageSize: 50);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetMovements_WithFromGreaterThanTo_ShouldReturnBadRequest()
    {
        var result = await _controller.GetMovements(
            CancellationToken.None,
            productId: null,
            from: new DateTime(2026, 02, 04),
            to: new DateTime(2026, 02, 01),
            page: 1,
            pageSize: 50);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetMovements_WithInvalidPage_ShouldReturnBadRequest()
    {
        var result = await _controller.GetMovements(
            CancellationToken.None,
            productId: null,
            from: null,
            to: null,
            page: 0,
            pageSize: 50);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetMovements_WithInvalidPageSize_ShouldReturnBadRequest()
    {
        var result = await _controller.GetMovements(
            CancellationToken.None,
            productId: null,
            from: null,
            to: null,
            page: 1,
            pageSize: 201);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetMovements_WithFilters_ShouldPassThroughToRepository()
    {
        var productId = Guid.NewGuid();
        var from = new DateTime(2026, 02, 01);
        var to = new DateTime(2026, 02, 04);

        _repo.PageItems = new List<InventoryMovement>();
        _repo.PageTotal = 0;

        _ = await _controller.GetMovements(
            CancellationToken.None,
            productId: productId,
            from: from,
            to: to,
            page: 2,
            pageSize: 20);

        Assert.AreEqual(productId, _repo.LastProductId);
        Assert.AreEqual(from, _repo.LastFrom);
        Assert.AreEqual(to, _repo.LastTo);
        Assert.AreEqual(2, _repo.LastPage);
        Assert.AreEqual(20, _repo.LastPageSize);
    }

    private sealed class FakeInventoryService : IInventoryService
    {
        public Guid? LastProductId { get; private set; }
        public int? LastDelta { get; private set; }

        public Task AdjustStockAsync(Guid productId, int delta, CancellationToken ct = default)
        {
            LastProductId = productId;
            LastDelta = delta;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeInventoryRepository : IInventoryRepository
    {
        public Guid? LastProductId { get; private set; }
        public DateTime? LastFrom { get; private set; }
        public DateTime? LastTo { get; private set; }
        public int? LastPage { get; private set; }
        public int? LastPageSize { get; private set; }

        public IReadOnlyList<InventoryMovement> PageItems { get; set; } = Array.Empty<InventoryMovement>();
        public int PageTotal { get; set; }

        public Task AddAsync(InventoryMovement movement, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<InventoryMovement>> GetAsync(Guid? productId = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<(IReadOnlyList<InventoryMovement> Items, int Total)> GetPageAsync(Guid? productId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            LastProductId = productId;
            LastFrom = from;
            LastTo = to;
            LastPage = page;
            LastPageSize = pageSize;

            return Task.FromResult((PageItems, PageTotal));
        }
    }
}
