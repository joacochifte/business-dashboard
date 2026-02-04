using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Infrastructure.Persistence;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Infrastructure.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _db;

    public SaleRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Sale> GetByIdAsync(Guid saleId)
    {
        return GetByIdInternalAsync(saleId);
    }

    public async Task<IEnumerable<Sale>> GetAllAsync()
    {
        return await _db.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Sale sale, CancellationToken ct = default)
    {
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken ct = default)
    {
        _db.Sales.Update(sale);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid saleId, CancellationToken ct = default)
    {
        var sale = await GetByIdInternalAsync(saleId, ct);
        _db.Sales.Remove(sale);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Sale> GetByIdInternalAsync(Guid saleId, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId, ct);
        if (sale is null)
            throw new InvalidOperationException("Sale not found.");
        return sale;
    }
}
