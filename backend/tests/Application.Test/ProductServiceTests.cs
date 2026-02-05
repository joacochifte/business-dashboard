using BusinessDashboard.Application.Products;
using BusinessDashboard.Domain.Products;
using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Infrastructure.Products;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Moq;

namespace Application.Test.Products;

[TestClass]
public class ProductServiceTests
{
    private Mock<IProductRepository> _repo = null!;
    private ProductService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repo = new Mock<IProductRepository>(MockBehavior.Strict);
        _service = new ProductService(_repo.Object);
    }

    [TestMethod]
    public async Task CreateProductAsync_ShouldCreateAndReturnId()
    {
        var request = new ProductCreationDto
        {
            Name = "Coca Cola 600ml",
            Price = 100m,
            InitialStock = 10,
            Description = "Bebida"
        };

        Product? captured = null;
        IReadOnlyList<InventoryMovement>? capturedMovements = null;
        _repo
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Product, IReadOnlyList<InventoryMovement>, CancellationToken>((p, m, _) =>
            {
                captured = p;
                capturedMovements = m;
            })
            .Returns(Task.CompletedTask);

        var id = await _service.CreateProductAsync(request);

        Assert.IsNotNull(captured);
        Assert.AreEqual(request.Name, captured.Name);
        Assert.AreEqual(request.Price, captured.Price);
        Assert.AreEqual(request.InitialStock, captured.Stock);
        Assert.AreEqual(request.Description, captured.Description);
        Assert.AreEqual(captured.Id, id);

        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(captured.Id, capturedMovements[0].ProductId);
        Assert.AreEqual(InventoryMovementType.In, capturedMovements[0].Type);
        Assert.AreEqual(InventoryMovementReason.Adjustment, capturedMovements[0].Reason);
        Assert.AreEqual(request.InitialStock, capturedMovements[0].Quantity);

        _repo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAllProductsAsync_ShouldMapToDto()
    {
        var products = new List<Product>
        {
            new Product(name: "A", price: 10m, initialStock: 5, description: "Desc A"),
            new Product(name: "B", price: 20m, initialStock: null, description: null)
        };

        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = (await _service.GetAllProductsAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("A", result[0].Name);
        Assert.AreEqual(5, result[0].Stock);
        Assert.AreEqual("B", result[1].Name);
        Assert.IsNull(result[1].Stock);

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetProductByIdAsync_ShouldMapToDto()
    {
        var id = Guid.NewGuid();
        var product = new Product(name: "A", price: 10m, initialStock: 5, description: "Desc");

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

        var dto = await _service.GetProductByIdAsync(id);

        Assert.AreEqual(product.Name, dto.Name);
        Assert.AreEqual(product.Price, dto.Price);
        Assert.AreEqual(product.Stock, dto.Stock);
        Assert.AreEqual(product.Description, dto.Description);

        _repo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [TestMethod]
    public async Task UpdateProductAsync_ShouldUpdateAndPersist()
    {
        var id = Guid.NewGuid();
        var product = new Product(name: "Old", price: 10m, initialStock: 5, description: "Old");
        var request = new ProductUpdateDto
        {
            Name = "New",
            Description = "New Desc",
            Price = 20m,
            Stock = 8,
            IsActive = true
        };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
        IReadOnlyList<InventoryMovement>? capturedMovements = null;
        _repo
            .Setup(r => r.UpdateAsync(product, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Product, IReadOnlyList<InventoryMovement>, CancellationToken>((_, m, _) => capturedMovements = m)
            .Returns(Task.CompletedTask);

        await _service.UpdateProductAsync(id, request);

        Assert.AreEqual(request.Name, product.Name);
        Assert.AreEqual(request.Description, product.Description);
        Assert.AreEqual(request.Price, product.Price);
        Assert.AreEqual(request.Stock, product.Stock);
        Assert.IsTrue(product.IsActive);

        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(product.Id, capturedMovements[0].ProductId);
        Assert.AreEqual(InventoryMovementType.In, capturedMovements[0].Type);
        Assert.AreEqual(3, capturedMovements[0].Quantity); // 5 -> 8

        _repo.Verify(r => r.GetByIdAsync(id), Times.Once);
        _repo.Verify(r => r.UpdateAsync(product, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteProductAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        var product = new Product(name: "A", price: 10m, initialStock: 5, description: null);
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

        IReadOnlyList<InventoryMovement>? capturedMovements = null;
        _repo
            .Setup(r => r.DeleteAsync(id, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, IReadOnlyList<InventoryMovement>, CancellationToken>((_, m, _) => capturedMovements = m)
            .Returns(Task.CompletedTask);

        await _service.DeleteProductAsync(id);

        Assert.IsNotNull(capturedMovements);
        Assert.AreEqual(1, capturedMovements.Count);
        Assert.AreEqual(product.Id, capturedMovements[0].ProductId);
        Assert.AreEqual(InventoryMovementType.Out, capturedMovements[0].Type);
        Assert.AreEqual(5, capturedMovements[0].Quantity);

        _repo.Verify(r => r.GetByIdAsync(id), Times.Once);
        _repo.Verify(r => r.DeleteAsync(id, It.IsAny<IReadOnlyList<InventoryMovement>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
