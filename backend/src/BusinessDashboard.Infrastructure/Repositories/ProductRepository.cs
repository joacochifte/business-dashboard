using BusinessDashboard.Domain.Products;
using BusinessDashboard.Infrastructure.Persistence;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Product> GetByIdAsync(Guid productId)
    {
        return GetByIdInternalAsync(productId);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await GetByIdInternalAsync(productId, ct);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Product> GetByIdInternalAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null)
            throw new InvalidOperationException("Product not found.");

        return product;
    }
}
