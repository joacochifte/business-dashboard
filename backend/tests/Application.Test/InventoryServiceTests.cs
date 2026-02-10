using BusinessDashboard.Application.Inventory;
using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Domain.Products;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Moq;

namespace Application.Test.Inventory;

[TestClass]
public class InventoryServiceTests
{
    private Mock<IProductRepository> _products = null!;
    private Mock<IInventoryRepository> _movements = null!;
    private InventoryService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _products = new Mock<IProductRepository>(MockBehavior.Strict);
        _movements = new Mock<IInventoryRepository>(MockBehavior.Strict);
        _service = new InventoryService(_products.Object, _movements.Object);
    }

    [TestMethod]
    public async Task AdjustStockAsync_WithZeroDelta_ShouldDoNothing()
    {
        await _service.AdjustStockAsync(Guid.NewGuid(), 0);

        _products.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _products.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _movements.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AdjustStockAsync_WithPositiveDelta_ShouldIncreaseStockAndCreateInMovement()
    {
        var productId = Guid.NewGuid();
        var product = new Product("Test", 10m, initialStock: 5);
        IReadOnlyList<InventoryMovement>? capturedMovements = null;

        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _products
            .Setup(r => r.UpdateAsync(product, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Product, IReadOnlyList<InventoryMovement>, CancellationToken>((_, m, _) => capturedMovements = m)
            .Returns(Task.CompletedTask);

        await _service.AdjustStockAsync(productId, 3);

        Assert.AreEqual(8, product.Stock);
        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(InventoryMovementType.In, capturedMovements[0].Type);
        Assert.AreEqual(InventoryMovementReason.Adjustment, capturedMovements[0].Reason);
        Assert.AreEqual(3, capturedMovements[0].Quantity);

        _products.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _products.Verify(
            r => r.UpdateAsync(product, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _movements.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AdjustStockAsync_WithNegativeDelta_ShouldDecreaseStockAndCreateOutMovement()
    {
        var productId = Guid.NewGuid();
        var product = new Product("Test", 10m, initialStock: 8);
        IReadOnlyList<InventoryMovement>? capturedMovements = null;

        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _products
            .Setup(r => r.UpdateAsync(product, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Product, IReadOnlyList<InventoryMovement>, CancellationToken>((_, m, _) => capturedMovements = m)
            .Returns(Task.CompletedTask);

        await _service.AdjustStockAsync(productId, -2);

        Assert.AreEqual(6, product.Stock);
        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(InventoryMovementType.Out, capturedMovements[0].Type);
        Assert.AreEqual(InventoryMovementReason.Adjustment, capturedMovements[0].Reason);
        Assert.AreEqual(2, capturedMovements[0].Quantity);

        _products.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _products.Verify(
            r => r.UpdateAsync(product, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _movements.VerifyNoOtherCalls();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task AdjustStockAsync_WithUntrackedStock_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        var product = new Product("Service", 10m, initialStock: null);

        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        await _service.AdjustStockAsync(productId, 1);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task AdjustStockAsync_WithInsufficientStock_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        var product = new Product("Test", 10m, initialStock: 1);

        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        await _service.AdjustStockAsync(productId, -2);
    }
}
