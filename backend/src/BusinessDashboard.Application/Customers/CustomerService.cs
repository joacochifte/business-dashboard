using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Domain.Common.Exceptions;
using BusinessDashboard.Infrastructure.Customers;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

namespace BusinessDashboard.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateCustomerAsync(CustomerCreationDto request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existing = await _repo.GetByEmailAsync(request.Email, ct);
            if (existing is not null)
                throw new BusinessRuleException($"A customer with email '{request.Email}' already exists.");
        }

        var customer = new Customer(
            name: request.Name,
            email: request.Email,
            phone: request.Phone,
            birthDate: ToUtc(request.BirthDate)
        );

        await _repo.AddAsync(customer, ct);
        return customer.Id;
    }

    public async Task DeleteCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(customerId, ct);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken ct = default)
    {
        var customers = await _repo.GetAllAsync(ct);
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _repo.GetByIdAsync(customerId, ct);
        return MapToDto(customer);
    }

    public async Task UpdateCustomerAsync(Guid customerId, CustomerUpdateDto request, CancellationToken ct = default)
    {
        var customer = await _repo.GetByIdAsync(customerId, ct);

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
        {
            var existing = await _repo.GetByEmailAsync(request.Email, ct);
            if (existing is not null)
                throw new BusinessRuleException($"A customer with email '{request.Email}' already exists.");
        }

        customer.SetName(request.Name);
        customer.SetEmail(request.Email);
        customer.SetPhone(request.Phone);
        customer.SetBirthDate(ToUtc(request.BirthDate));

        if (!request.IsActive)
            customer.Deactivate();
        else
            customer.Activate();

        await _repo.UpdateAsync(customer, ct);
    }

    private static DateTime? ToUtc(DateTime? dt) =>
        dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

    private static CustomerDto MapToDto(Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        BirthDate = customer.BirthDate,
        IsActive = customer.IsActive,
        LastPurchaseDate = customer.LastPurchaseDate,
        TotalPurchases = customer.TotalPurchases,
        TotalLifetimeValue = customer.TotalLifetimeValue
    };
}
