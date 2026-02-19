using BusinessDashboard.Infrastructure.Persistence;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Infrastructure.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var salesQuery = ApplySalesFilter(_db.Sales.AsNoTracking(), from, to);
        var costsQuery = ApplyCostsFilter(_db.Costs.AsNoTracking(), from, to);

        var salesCount = await salesQuery.CountAsync(ct);
        var revenueTotal = await salesQuery.SumAsync(s => (decimal?)s.Total, ct) ?? 0m;
        var costsTotal = await costsQuery.SumAsync(c => (decimal?)c.Amount, ct) ?? 0m;
        var gains = revenueTotal - costsTotal;
        var avgTicket = salesCount == 0 ? 0m : revenueTotal / salesCount;

        return new DashboardSummaryDto
        {
            RevenueTotal = revenueTotal,
            Gains = gains,
            SalesCount = salesCount,
            AvgTicket = avgTicket
        };
    }

    public async Task<SalesByPeriodDto> GetSalesByPeriodAsync(string groupBy, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var normalized = NormalizeGroupBy(groupBy);
        var sales = await ApplySalesFilter(_db.Sales.AsNoTracking(), from, to)
            .Select(s => new { s.CreatedAt, s.Total })
            .ToListAsync(ct);

        var points = sales
            .GroupBy(s => GetPeriodStart(s.CreatedAt, normalized))
            .Select(g => new SalesByPeriodPointDto
            {
                PeriodStart = g.Key,
                SalesCount = g.Count(),
                Revenue = g.Sum(x => x.Total)
            })
            .OrderBy(p => p.PeriodStart)
            .ToList();

        return new SalesByPeriodDto
        {
            GroupBy = normalized,
            Points = points
        };
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int limit = 10, DateTime? from = null, DateTime? to = null, string sortBy = "revenue", CancellationToken ct = default)
    {
        if (limit < 1) limit = 1;
        if (limit > 50) limit = 50;

        var sales = await ApplySalesFilter(_db.Sales.AsNoTracking(), from, to)
            .Include(s => s.Items)
            .ToListAsync(ct);

        var grouped = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * (x.SpecialPrice ?? x.UnitPrice))
            });

        // Sort by revenue or quantity based on sortBy parameter
        var ordered = sortBy?.ToLowerInvariant() == "quantity"
            ? grouped.OrderByDescending(x => x.Quantity).ThenByDescending(x => x.Revenue)
            : grouped.OrderByDescending(x => x.Revenue).ThenByDescending(x => x.Quantity);

        var topItems = ordered.Take(limit).ToList();

        var ids = topItems.Select(x => x.ProductId).ToList();
        var names = await _db.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(ct);

        var nameMap = names.ToDictionary(x => x.Id, x => x.Name);

        return topItems.Select(x => new TopProductDto
        {
            ProductId = x.ProductId,
            ProductName = nameMap.GetValueOrDefault(x.ProductId, string.Empty),
            Quantity = x.Quantity,
            Revenue = x.Revenue
        }).ToList();
    }

    private static IQueryable<Sale> ApplySalesFilter(IQueryable<Sale> query, DateTime? from, DateTime? to)
    {
        if (from is not null)
            query = query.Where(s => s.CreatedAt >= from.Value);

        if (to is not null)
            query = query.Where(s => s.CreatedAt <= to.Value);

        // Exclude debts from dashboard calculations
        query = query.Where(s => !s.IsDebt);

        return query;
    }

    private static IQueryable<Cost> ApplyCostsFilter(IQueryable<Cost> query, DateTime? from, DateTime? to)
    {
        if (from is not null)
            query = query.Where(c => c.DateIncurred >= from.Value);

        if (to is not null)
            query = query.Where(c => c.DateIncurred <= to.Value);

        return query;
    }

    private static string NormalizeGroupBy(string groupBy)
    {
        if (string.IsNullOrWhiteSpace(groupBy))
            return "day";

        return groupBy.Trim().ToLowerInvariant() switch
        {
            "day" => "day",
            "week" => "week",
            "month" => "month",
            _ => throw new ArgumentException("groupBy must be 'day', 'week', or 'month'.", nameof(groupBy))
        };
    }

    private static DateTime GetPeriodStart(DateTime dt, string groupBy)
    {
        dt = dt.ToUniversalTime();

        return groupBy switch
        {
            "day" => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc),
            "month" => new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            "week" => StartOfWeekUtc(dt, DayOfWeek.Monday),
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy))
        };
    }

    public async Task<IReadOnlyList<CustomerSalesDto>> GetSalesByCustomerAsync(int limit = 10, DateTime? from = null, DateTime? to = null, bool? excludeDebts = true, CancellationToken ct = default)
    {
        if (limit < 1) limit = 1;
        if (limit > 50) limit = 50;

        var salesQuery = ApplySalesFilterWithDebtOption(_db.Sales.AsNoTracking(), from, to, excludeDebts);
        
        var salesByCustomer = await salesQuery
            .Include(s => s.Customer)
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                CustomerName = g.First().Customer != null && g.First().Customer.Name != null ? g.First().Customer.Name : "Unknown",
                SalesCount = g.Count()
            })
            .OrderByDescending(x => x.SalesCount)
            .Take(limit)
            .ToListAsync(ct);

        return salesByCustomer
            .Select(x => new CustomerSalesDto
            {
                CustomerId = x.CustomerId.GetValueOrDefault(),
                CustomerName = x.CustomerName,
                SalesCount = x.SalesCount
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CustomerSpendingDto>> GetSpendingByCustomerAsync(int limit = 10, DateTime? from = null, DateTime? to = null, bool? excludeDebts = true, CancellationToken ct = default)
    {
        if (limit < 1) limit = 1;
        if (limit > 50) limit = 50;

        var salesQuery = ApplySalesFilterWithDebtOption(_db.Sales.AsNoTracking(), from, to, excludeDebts);

        var spendingByCustomer = await salesQuery
            .Include(s => s.Customer)
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                CustomerName = g.First().Customer != null? g.First().Customer.Name : "Unknown",
                TotalSpent = g.Sum(s => s.Total)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(limit)
            .ToListAsync(ct);

        return spendingByCustomer
            .Select(x => new CustomerSpendingDto
            {
                CustomerId = x.CustomerId.GetValueOrDefault(),
                CustomerName = x.CustomerName,
                TotalSpent = x.TotalSpent
            })
            .ToList();
    }

    private static IQueryable<Sale> ApplySalesFilterWithDebtOption(IQueryable<Sale> query, DateTime? from, DateTime? to, bool? excludeDebts)
    {
        if (from is not null)
            query = query.Where(s => s.CreatedAt >= from.Value);

        if (to is not null)
            query = query.Where(s => s.CreatedAt <= to.Value);

        if (excludeDebts.HasValue && excludeDebts.Value)
            query = query.Where(s => !s.IsDebt);

        return query;
    }

    private static DateTime StartOfWeekUtc(DateTime dt, DayOfWeek startOfWeek)
    {
        var date = DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
        var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff);
    }
}
