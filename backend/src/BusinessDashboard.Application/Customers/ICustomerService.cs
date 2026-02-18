using BusinessDashboard.Infrastructure.Customers;

namespace BusinessDashboard.Application.Customers;

public interface ICustomerService
{
    Task<Guid> CreateCustomerAsync(CustomerCreationDto request, CancellationToken ct = default);
    Task DeleteCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken ct = default);
    Task<CustomerDto> GetCustomerByIdAsync(Guid customerId, CancellationToken ct = default);
    Task UpdateCustomerAsync(Guid customerId, CustomerUpdateDto request, CancellationToken ct = default);
}
