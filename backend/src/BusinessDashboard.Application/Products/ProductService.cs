using BusinessDashboard.Domain.Products;
using BusinessDashboard.Infrastructure.Products;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

namespace BusinessDashboard.Application.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateProductAsync(ProductCreationDto request, CancellationToken ct = default)
    {
        var product = new Product(
            name: request.Name,
            price: request.Price,
            initialStock: request.InitialStock,
            description: request.Description
        );

        await _repo.AddAsync(product, ct);
        return product.Id;
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(productId, ct);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default)
    {
        var products = await _repo.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            IsActive = p.IsActive
        });
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(productId);
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        };
    }

    public async Task UpdateProductAsync(Guid productId, ProductUpdateDto request, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(productId);
        product.Update(
            name: request.Name,
            description: request.Description,
            price: request.Price,
            stock: request.Stock,
            isActive: request.IsActive
        );
        await _repo.UpdateAsync(product, ct);
    }
}
