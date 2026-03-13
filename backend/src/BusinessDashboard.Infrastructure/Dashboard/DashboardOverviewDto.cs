namespace BusinessDashboard.Infrastructure.Dashboard;

public class DashboardOverviewDto
{
    public decimal RevenueTotal { get; init; }
    public decimal CostsTotal { get; init; }
    public decimal Gains { get; init; }
    public decimal MarginPct { get; init; }
    public int SalesCount { get; init; }
    public int UnitsSold { get; init; }
    public decimal AvgTicket { get; init; }
    public decimal DebtsTotal { get; init; }
    public decimal DebtRatioPct { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
    public CustomerSpendingDto? TopCustomer { get; init; }
    public TopProductDto? TopProductByQuantity { get; init; }
    public DashboardComparisonDto Comparison { get; init; } = new();
    public IReadOnlyList<DashboardAlertDto> Alerts { get; init; } = Array.Empty<DashboardAlertDto>();
}
