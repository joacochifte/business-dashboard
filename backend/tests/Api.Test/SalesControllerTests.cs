using BusinessDashboard.Application.Sales;
using BusinessDashboard.Infrastructure.Products;
using BusinessDashboard.Infrastructure.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class SalesControllerTests
{
    private FakeSalesService _service = null!;
    private SalesController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeSalesService();
        _controller = new SalesController(_service);
    }

    [TestMethod]
    public async Task GetSales_ShouldReturnOkWithSales()
    {
        var expected = new List<SaleDto>
        {
            new() { Id = Guid.NewGuid(), Total = 100m, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Total = 200m, CreatedAt = DateTime.UtcNow }
        };
        _service.GetAllSalesResult = expected;

        var result = await _controller.GetSales();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<SaleDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count());
    }

    [TestMethod]
    public async Task GetSaleById_ShouldReturnOkWithSale()
    {
        var id = Guid.NewGuid();
        var expected = new SaleDto { Id = id, Total = 150m, CreatedAt = DateTime.UtcNow };
        _service.GetByIdResult = expected;

        var result = await _controller.GetSaleById(id);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as SaleDto;
        Assert.IsNotNull(value);
        Assert.AreEqual(id, value.Id);
    }

    [TestMethod]
    public async Task CreateSale_ShouldReturnCreatedAtAction()
    {
        var id = Guid.NewGuid();
        _service.CreateResult = id;
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = Guid.NewGuid(), UnitPrice = 10m, Quantity = 1 }
            },
            Total = 100m
        };

        var result = await _controller.CreateSale(request);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(nameof(SalesController.GetSaleById), created.ActionName);
        Assert.IsNotNull(created.RouteValues);
        Assert.AreEqual(id, created.RouteValues["id"]);
    }

    [TestMethod]
    public async Task UpdateSale_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var request = new SaleUpdateDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = Guid.NewGuid(), UnitPrice = 10m, Quantity = 1 }
            },
            Total = 50m
        };

        var result = await _controller.UpdateSale(id, request);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastUpdatedId);
    }

    [TestMethod]
    public async Task DeleteSale_ShouldReturnOk()
    {
        var id = Guid.NewGuid();

        var result = await _controller.DeleteSale(id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastDeletedId);
    }

    private sealed class FakeSalesService : ISalesService
    {
        public IEnumerable<SaleDto> GetAllSalesResult { get; set; } = Array.Empty<SaleDto>();
        public SaleDto GetByIdResult { get; set; } = new();
        public Guid CreateResult { get; set; } = Guid.NewGuid();
        public Guid? LastUpdatedId { get; private set; }
        public Guid? LastDeletedId { get; private set; }

        public Task<Guid> CreateSaleAsync(SaleCreationDto request, CancellationToken ct = default)
        {
            return Task.FromResult(CreateResult);
        }

        public Task DeleteSaleAsync(Guid saleId, CancellationToken ct = default)
        {
            LastDeletedId = saleId;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SaleDto>> GetAllSalesAsync(bool? isDebt = null, CancellationToken ct = default)
        {
            return Task.FromResult(GetAllSalesResult);
        }

        public Task<SaleDto> GetSaleByIdAsync(Guid saleId, CancellationToken ct = default)
        {
            return Task.FromResult(GetByIdResult);
        }

        public Task UpdateSaleAsync(Guid saleId, SaleUpdateDto request, CancellationToken ct = default)
        {
            LastUpdatedId = saleId;
            return Task.CompletedTask;
        }
    }
}
