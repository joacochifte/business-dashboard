namespace BusinessDashboard.Application.Costs;
using BusinessDashboard.Infrastructure.Costs;
using BusinessDashboard.Domain.Costs;

public interface ICostService
{
    Task<Guid> AddCostAsync(CostCreationDto costDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostSummaryDto>> GetCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Cost?> GetCostByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateCostAsync(Guid id, CostCreationDto costDto, CancellationToken cancellationToken = default);
    Task DeleteCostAsync(Guid id, CancellationToken cancellationToken = default);
}
