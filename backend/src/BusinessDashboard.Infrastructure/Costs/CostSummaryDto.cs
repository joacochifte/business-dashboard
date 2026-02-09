namespace BusinessDashboard.Infrastructure.Costs;

public class CostSummaryDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateIncurred { get; set; }
}