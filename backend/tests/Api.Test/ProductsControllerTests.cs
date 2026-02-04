using BusinessDashboard.Application.Products;
using BusinessDashboard.Infrastructure.Products;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class ProductsControllerTests
{
    private FakeProductService _service = null!;
    private ProductsController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeProductService();
        _controller = new ProductsController(_service);
    }

    [TestMethod]
    public async Task GetProducts_ShouldReturnOkWithProducts()
    {
        var expected = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Name = "A", Price = 10m, Stock = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "B", Price = 20m, Stock = 2, IsActive = true }
        };
        _service.GetAllProductsResult = expected;

        var result = await _controller.GetProducts();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<ProductDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count());
    }

    [TestMethod]
    public async Task GetProductById_ShouldReturnOkWithProduct()
    {
        var id = Guid.NewGuid();
        var expected = new ProductDto { Id = id, Name = "A", Price = 10m, Stock = 1, IsActive = true };
        _service.GetByIdResult = expected;

        var result = await _controller.GetProductById(id);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as ProductDto;
        Assert.IsNotNull(value);
        Assert.AreEqual(id, value.Id);
    }

    [TestMethod]
    public async Task CreateProduct_ShouldReturnCreatedAtAction()
    {
        var id = Guid.NewGuid();
        _service.CreateResult = id;
        var request = new ProductCreationDto { Name = "A", Price = 10m, InitialStock = 1 };

        var result = await _controller.CreateProduct(request);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(nameof(ProductsController.GetProductById), created.ActionName);
        Assert.IsNotNull(created.RouteValues);
        Assert.AreEqual(id, created.RouteValues["id"]);
    }

    [TestMethod]
    public async Task UpdateProduct_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var request = new ProductUpdateDto
        {
            Name = "A",
            Price = 10m,
            Stock = 1,
            IsActive = true
        };

        var result = await _controller.UpdateProduct(id, request);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastUpdatedId);
    }

    [TestMethod]
    public async Task DeleteProduct_ShouldReturnOk()
    {
        var id = Guid.NewGuid();

        var result = await _controller.DeleteProduct(id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastDeletedId);
    }

    private sealed class FakeProductService : IProductService
    {
        public IEnumerable<ProductDto> GetAllProductsResult { get; set; } = Array.Empty<ProductDto>();
        public ProductDto GetByIdResult { get; set; } = new();
        public Guid CreateResult { get; set; } = Guid.NewGuid();
        public Guid? LastUpdatedId { get; private set; }
        public Guid? LastDeletedId { get; private set; }

        public Task<Guid> CreateProductAsync(ProductCreationDto request, CancellationToken ct = default)
        {
            return Task.FromResult(CreateResult);
        }

        public Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
        {
            LastDeletedId = productId;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default)
        {
            return Task.FromResult(GetAllProductsResult);
        }

        public Task<ProductDto> GetProductByIdAsync(Guid productId, CancellationToken ct = default)
        {
            return Task.FromResult(GetByIdResult);
        }

        public Task UpdateProductAsync(Guid productId, ProductUpdateDto request, CancellationToken ct = default)
        {
            LastUpdatedId = productId;
            return Task.CompletedTask;
        }
    }
}
