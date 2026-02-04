namespace BusinessDashboard.Application.Dashboard;
using BusinessDashboard.Infrastructure.Dashboard;
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<SalesByPeriodDto> GetSalesByPeriodAsync(string groupBy, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int limit = 10, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
}

