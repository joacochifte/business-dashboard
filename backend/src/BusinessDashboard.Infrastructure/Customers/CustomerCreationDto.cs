namespace BusinessDashboard.Infrastructure.Customers;

public class CustomerCreationDto
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateTime? BirthDate { get; init; }
}
