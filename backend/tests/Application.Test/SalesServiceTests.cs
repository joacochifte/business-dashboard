using BusinessDashboard.Application.Products;
using BusinessDashboard.Application.Sales;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Infrastructure.Sales;
using Moq;

namespace Application.Test.Sales;

[TestClass]
public class SalesServiceTests
{
    private Mock<ISaleRepository> _repo = null!;
    private SalesService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repo = new Mock<ISaleRepository>(MockBehavior.Strict);
        _service = new SalesService(_repo.Object);
    }

    [TestMethod]
    public async Task CreateSaleAsync_ShouldCreateAndReturnId()
    {
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m },
                new SaleItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }
            },
            Total = 200m
        };

        Sale? captured = null;
        _repo
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((s, _) => captured = s)
            .Returns(Task.CompletedTask);

        var id = await _service.CreateSaleAsync(request);

        Assert.IsNotNull(captured);
        Assert.AreEqual(2, captured.Items.Count);
        Assert.AreEqual(200m, captured.Total);
        Assert.AreEqual(captured.Id, id);

        _repo.Verify(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task CreateSaleAsync_WithNoProducts_ShouldThrowInvalidOperationException()
    {
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>(),
            Total = 100m
        };

        await _service.CreateSaleAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public async Task CreateSaleAsync_WithZeroTotal_ShouldThrowArgumentOutOfRangeException()
    {
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }
            },
            Total = 0m
        };

        await _service.CreateSaleAsync(request);
    }

    [TestMethod]
    public async Task GetAllSalesAsync_ShouldMapToDto()
    {
        var sales = new List<Sale>
        {
            new Sale(new[] { new SaleItem(Guid.NewGuid(), 1, 50m) }),
            new Sale(new[] { new SaleItem(Guid.NewGuid(), 2, 30m) })
        };

        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

        var result = (await _service.GetAllSalesAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(sales[0].Total, result[0].Total);
        Assert.AreEqual(sales[1].Total, result[1].Total);

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetSaleByIdAsync_ShouldMapToDto()
    {
        var saleId = Guid.NewGuid();
        var sale = new Sale(new[] { new SaleItem(Guid.NewGuid(), 1, 80m) });

        _repo.Setup(r => r.GetByIdAsync(saleId)).ReturnsAsync(sale);

        var dto = await _service.GetSaleByIdAsync(saleId);

        Assert.AreEqual(sale.Total, dto.Total);
        Assert.AreEqual(sale.CreatedAt, dto.CreatedAt);
        Assert.AreEqual(sale.Items.First().ProductId, dto.ProductId);

        _repo.Verify(r => r.GetByIdAsync(saleId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteSaleAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.DeleteSaleAsync(id);

        _repo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task UpdateSaleAsync_ShouldThrowInvalidOperationException()
    {
        var request = new SaleUpdateDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }
            },
            Total = 50m
        };

        await _service.UpdateSaleAsync(Guid.NewGuid(), request);
    }
}
