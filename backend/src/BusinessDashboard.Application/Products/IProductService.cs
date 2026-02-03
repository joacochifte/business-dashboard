using System.Runtime.CompilerServices;
using BusinessDashboard.Infrastructure.Products;
namespace BusinessDashboard.Application.Products;

public interface IProductService
{
    Task<Guid> CreateProductAsync(ProductCreationDto request, CancellationToken ct = default);
    Task DeleteProductAsync(Guid productId, CancellationToken ct = default);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default);
    Task<ProductDto> GetProductByIdAsync(Guid productId,  CancellationToken ct = default);
    Task UpdateProductAsync(Guid productId, ProductUpdateDto request, CancellationToken ct = default);
}