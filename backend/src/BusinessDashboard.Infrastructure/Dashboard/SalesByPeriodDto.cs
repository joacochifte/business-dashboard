namespace BusinessDashboard.Infrastructure.Dashboard;

public class SalesByPeriodDto
{
    public string GroupBy { get; init; } = "day";
    public IReadOnlyList<SalesByPeriodPointDto> Points { get; init; } = Array.Empty<SalesByPeriodPointDto>();
}



