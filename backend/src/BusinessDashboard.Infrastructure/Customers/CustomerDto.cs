namespace BusinessDashboard.Infrastructure.Customers;

public class CustomerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateTime? BirthDate { get; init; }
    public bool IsActive { get; init; }
    public DateTime LastPurchaseDate { get; init; }
    public int TotalPurchases { get; init; }
    public decimal TotalLifetimeValue { get; init; }
}
