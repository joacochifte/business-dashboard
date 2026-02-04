using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Infrastructure.Persistence;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Infrastructure.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;
    public InventoryRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(InventoryMovement movement, CancellationToken ct = default)
    {
        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await GetAsync(productId: productId, ct: ct);
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetAsync(Guid? productId = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = BuildQuery(productId, from, to);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<InventoryMovement> Items, int Total)> GetPageAsync(
        Guid? productId = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 200) pageSize = 200;

        var query = BuildQuery(productId, from, to);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    private IQueryable<InventoryMovement> BuildQuery(Guid? productId, DateTime? from, DateTime? to)
    {
        var query = _db.InventoryMovements.AsNoTracking().AsQueryable();

        if (productId is not null)
            query = query.Where(m => m.ProductId == productId.Value);

        if (from is not null)
            query = query.Where(m => m.CreatedAt >= from.Value);

        if (to is not null)
            query = query.Where(m => m.CreatedAt <= to.Value);

        return query;
    }
}
