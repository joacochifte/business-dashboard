using BusinessDashboard.Domain.Customers;

namespace BusinessDashboard.Infrastructure.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByIdOrDefaultAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task UpdateAsync(Customer customer, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
