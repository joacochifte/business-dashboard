namespace BusinessDashboard.Application.Dashboard;
using BusinessDashboard.Infrastructure.Dashboard;
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<DashboardOverviewDto> GetOverviewAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<DashboardPerformanceSeriesDto> GetPerformanceSeriesAsync(
        string groupBy,
        DateTime? from = null,
        DateTime? to = null,
        int tzOffsetMinutes = 0,
        IReadOnlyList<int>? compareYearOffsets = null,
        IReadOnlyList<string>? compareMonths = null,
        bool includeForecast = false,
        int forecastPeriods = 3,
        CancellationToken ct = default);
    Task<SalesByPeriodDto> GetSalesByPeriodAsync(string groupBy, DateTime? from = null, DateTime? to = null, int tzOffsetMinutes = 0, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int limit = 10, DateTime? from = null, DateTime? to = null, string sortBy = "revenue", CancellationToken ct = default);
    Task<IReadOnlyList<CustomerSalesDto>> GetSalesByCustomerAsync(int limit = 10, DateTime? from = null, DateTime? to = null, bool? excludeDebts = true, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerSpendingDto>> GetSpendingByCustomerAsync(int limit = 10, DateTime? from = null, DateTime? to = null, bool? excludeDebts = true, CancellationToken ct = default);
}
