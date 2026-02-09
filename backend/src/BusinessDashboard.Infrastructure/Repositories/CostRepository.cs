namespace BusinessDashboard.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

public class CostRepository : ICostRepository
{
     private readonly AppDbContext _db;

    public CostRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddCostAsync(Cost cost, CancellationToken cancellationToken = default)
    {
        _db.Costs.Add(cost);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cost = await GetByIdInternalAsync(id, cancellationToken);
        _db.Costs.Remove(cost);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cost> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var cost = await _db.Costs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (cost == null)
            throw new KeyNotFoundException($"Cost with ID {id} not found.");

        return cost;
    }

    public async Task<Cost?> GetCostByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Costs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Cost>> GetCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        return await _db.Costs
            .AsNoTracking()
            .Where(c => (!startDate.HasValue || c.DateIncurred >= startDate.Value) &&
                        (!endDate.HasValue || c.DateIncurred <= endDate.Value))
            .OrderByDescending(c => c.DateIncurred)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateCostAsync(Cost cost, CancellationToken ct = default)
    {
         await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            _db.Costs.Update(cost);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}