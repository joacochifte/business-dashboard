namespace BusinessDashboard.Infrastructure.Repositories.Interfaces;

using BusinessDashboard.Domain.Sales;

public interface ISaleRepository
{
    Task<Sale> GetByIdAsync(Guid saleId);
    Task<IEnumerable<Sale>> GetAllAsync();
    Task AddAsync(Sale sale, CancellationToken ct = default);
    Task UpdateAsync(Sale sale, CancellationToken ct = default);
    Task DeleteAsync(Guid saleId, CancellationToken ct = default);
}
