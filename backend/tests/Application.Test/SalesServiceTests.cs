using BusinessDashboard.Application.Sales;
using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Domain.Products;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Infrastructure.Sales;
using Moq;
using BusinessDashboard.Domain.Common.Exceptions;

namespace Application.Test.Sales;

[TestClass]
public class SalesServiceTests
{
    private Mock<ISaleRepository> _repo = null!;
    private Mock<IProductRepository> _products = null!;
    private Mock<ICustomerRepository> _customers = null!;
    private SalesService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repo = new Mock<ISaleRepository>(MockBehavior.Strict);
        _products = new Mock<IProductRepository>(MockBehavior.Strict);
        _customers = new Mock<ICustomerRepository>(MockBehavior.Strict);
        _service = new SalesService(_repo.Object, _products.Object, _customers.Object);
    }

    [TestMethod]
    public async Task CreateSaleAsync_ShouldCreateAndReturnId()
    {
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = p1, Quantity = 1, UnitPrice = 100m },
                new SaleItemDto { ProductId = p2, Quantity = 1, UnitPrice = 100m }
            },
            Total = 200m
        };

        Sale? captured = null;
        _products.Setup(r => r.GetByIdAsync(p1)).ReturnsAsync(new Product("P1", 1m, initialStock: 10));
        _products.Setup(r => r.GetByIdAsync(p2)).ReturnsAsync(new Product("P2", 1m, initialStock: 10));

        _repo
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, IReadOnlyList<InventoryMovement>, CancellationToken>((s, _, _) => captured = s)
            .Returns(Task.CompletedTask);

        var id = await _service.CreateSaleAsync(request);

        Assert.IsNotNull(captured);
        Assert.AreEqual(2, captured.Items.Count);
        Assert.AreEqual(200m, captured.Total);
        Assert.AreEqual(captured.Id, id);

        _repo.Verify(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
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
    public async Task CreateSaleAsync_WithZeroTotal_ShouldCreateSale()
    {
        var pid = Guid.NewGuid();
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = pid, Quantity = 1, UnitPrice = 100m }
            },
            Total = 0m
        };

        _products.Setup(r => r.GetByIdAsync(pid)).ReturnsAsync(new Product("P1", 1m, initialStock: 10));

        _repo.Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = await _service.CreateSaleAsync(request);

        Assert.AreNotEqual(Guid.Empty, id);
        _repo.Verify(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
    public async Task CreateSaleAsync_WithTotalMismatch_ShouldThrowInvalidOperationException()
    {
        var pid = Guid.NewGuid();
        var request = new SaleCreationDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = pid, Quantity = 1, UnitPrice = 100m }
            },
            Total = 999m
        };

        _products.Setup(r => r.GetByIdAsync(pid)).ReturnsAsync(new Product("P1", 1m, initialStock: 10));

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
        CollectionAssert.AreEquivalent(
            sales.Select(s => s.Total).ToList(),
            result.Select(r => r.Total).ToList()
        );

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
        Assert.AreEqual(sale.Id, dto.Id);

        _repo.Verify(r => r.GetByIdAsync(saleId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteSaleAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sale = new Sale(new[] { new SaleItem(productId, 2, 10m) });
        var product = new Product("P1", 10m, initialStock: 5);

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(sale);
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        IReadOnlyList<InventoryMovement>? capturedMovements = null;
        _repo
            .Setup(r => r.DeleteAsync(id, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, IReadOnlyList<InventoryMovement>, CancellationToken>((_, m, _) => capturedMovements = m)
            .Returns(Task.CompletedTask);

        await _service.DeleteSaleAsync(id);

        Assert.AreEqual(7, product.Stock);
        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(InventoryMovementType.In, capturedMovements[0].Type);
        Assert.AreEqual(InventoryMovementReason.Sale, capturedMovements[0].Reason);
        Assert.AreEqual(2, capturedMovements[0].Quantity);

        _repo.Verify(r => r.GetByIdAsync(id), Times.Once);
        _products.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _repo.Verify(r => r.DeleteAsync(id, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateSaleAsync_ShouldUpdateAndPersist()
    {
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sale = new Sale(new[] { new SaleItem(productId, 1, 50m) });
        var request = new SaleUpdateDto
        {
            Items = new List<SaleItemDto>
            {
                new SaleItemDto { ProductId = productId, Quantity = 2, UnitPrice = 20m }
            },
            Total = 40m,
            CustomerId = null,
            PaymentMethod = "Cash"
        };

        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product("P1", 10m, initialStock: 10));
        _repo.Setup(r => r.GetByIdAsync(saleId)).ReturnsAsync(sale);
        IReadOnlyList<InventoryMovement>? capturedMovements = null;
        _repo
            .Setup(r => r.UpdateAsync(sale, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, IReadOnlyList<InventoryMovement>, CancellationToken>((_, m, _) => capturedMovements = m)
            .Returns(Task.CompletedTask);

        await _service.UpdateSaleAsync(saleId, request);

        Assert.IsNull(sale.CustomerName);
        Assert.AreEqual("Cash", sale.PaymentMethod);
        Assert.AreEqual(40m, sale.Total);
        Assert.AreEqual(1, sale.Items.Count);
        Assert.AreEqual(productId, sale.Items.First().ProductId);
        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(InventoryMovementType.Out, capturedMovements[0].Type);
        Assert.AreEqual(InventoryMovementReason.Sale, capturedMovements[0].Reason);
        Assert.AreEqual(1, capturedMovements[0].Quantity);
        _repo.Verify(r => r.GetByIdAsync(saleId), Times.Once);
        _products.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _repo.Verify(r => r.UpdateAsync(sale, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
