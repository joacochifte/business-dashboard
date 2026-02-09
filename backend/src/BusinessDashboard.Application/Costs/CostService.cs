namespace BusinessDashboard.Application.Costs;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Infrastructure.Costs;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

public class CostService : ICostService
{
    private readonly ICostRepository _db;

    public CostService(ICostRepository db)
    {
        _db = db;
    }
    public async Task<Guid> AddCostAsync(CostCreationDto costDto, CancellationToken cancellationToken = default)
    {
        var cost = new Cost(costDto.Name, costDto.Amount, costDto.DateIncurred, costDto.Description);
        await _db.AddCostAsync(cost, cancellationToken);
        return cost.Id;
    }

    public async Task DeleteCostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _db.DeleteCostAsync(id, cancellationToken);
    }

    public async Task<Cost?> GetCostByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.GetCostByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<CostSummaryDto>> GetCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        return (await _db.GetCostsAsync(startDate, endDate, cancellationToken))
            .Select(c => new CostSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Amount = c.Amount,
                DateIncurred = c.DateIncurred,
            });
    }

    public async Task UpdateCostAsync(Guid id, CostCreationDto costDto, CancellationToken cancellationToken = default)
    {
        var existingCost = await _db.GetCostByIdAsync(id, cancellationToken);
        if (existingCost == null)
            throw new KeyNotFoundException($"Cost with ID {id} not found.");

        existingCost.Name = costDto.Name;
        existingCost.Amount = costDto.Amount;
        existingCost.DateIncurred = costDto.DateIncurred;
        existingCost.Description = costDto.Description;

        await _db.UpdateCostAsync(existingCost, cancellationToken);
    }
}
