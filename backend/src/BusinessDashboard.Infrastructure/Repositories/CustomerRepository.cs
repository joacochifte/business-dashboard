using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Infrastructure.Persistence;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (customer is null)
            throw new KeyNotFoundException($"Customer with ID {id} not found.");

        return customer;
    }

    public async Task<Customer?> GetByIdOrDefaultAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email, ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await GetByIdAsync(id, ct);
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(ct);
    }
}
