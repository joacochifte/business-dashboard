namespace BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Domain.Costs;

public interface ICostRepository
{
    Task AddCostAsync(Cost cost, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cost>> GetCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Cost?> GetCostByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateCostAsync(Cost cost, CancellationToken cancellationToken = default);
    Task DeleteCostAsync(Guid id, CancellationToken cancellationToken = default);
}