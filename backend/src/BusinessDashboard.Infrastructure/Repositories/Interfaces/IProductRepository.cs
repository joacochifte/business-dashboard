namespace BusinessDashboard.Infrastructure.Repositories.Interfaces;

using BusinessDashboard.Domain.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProductRepository
{
    Task<Product> GetByIdAsync(Guid productId);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid productId, CancellationToken ct = default);
}
