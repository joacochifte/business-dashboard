using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Infrastructure.Dashboard;
using BusinessDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private const int LowStockThreshold = 5;
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

    public async Task<DashboardOverviewDto> GetOverviewAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var previousRange = GetPreviousRange(from, to);
        var currentSnapshot = await BuildSalesSnapshotAsync(from, to, includeInsights: true, ct);
        var costsTotal = await GetCostsTotalAsync(from, to, ct);
        var debtsTotal = await GetDebtsTotalAsync(from, to, ct);
        var stockRisk = await GetStockRiskAsync(ct);

        var gains = currentSnapshot.RevenueTotal - costsTotal;
        var marginPct = currentSnapshot.RevenueTotal == 0m ? 0m : Math.Round((gains / currentSnapshot.RevenueTotal) * 100m, 2);
        var totalSalesWithDebt = currentSnapshot.RevenueTotal + debtsTotal;
        var debtRatioPct = totalSalesWithDebt == 0m ? 0m : Math.Round((debtsTotal / totalSalesWithDebt) * 100m, 2);

        var comparison = new DashboardComparisonDto();
        if (previousRange is not null)
        {
            var previousSnapshot = await BuildSalesSnapshotAsync(previousRange.Value.From, previousRange.Value.To, includeInsights: false, ct);
            var previousCostsTotal = await GetCostsTotalAsync(previousRange.Value.From, previousRange.Value.To, ct);
            var previousGains = previousSnapshot.RevenueTotal - previousCostsTotal;

            comparison = new DashboardComparisonDto
            {
                RevenueDeltaPct = CalculateDeltaPct(currentSnapshot.RevenueTotal, previousSnapshot.RevenueTotal),
                CostsDeltaPct = CalculateDeltaPct(costsTotal, previousCostsTotal),
                GainsDeltaPct = CalculateDeltaPct(gains, previousGains),
                SalesCountDeltaPct = CalculateDeltaPct(currentSnapshot.SalesCount, previousSnapshot.SalesCount),
                UnitsSoldDeltaPct = CalculateDeltaPct(currentSnapshot.UnitsSold, previousSnapshot.UnitsSold)
            };
        }

        return new DashboardOverviewDto
        {
            RevenueTotal = currentSnapshot.RevenueTotal,
            CostsTotal = costsTotal,
            Gains = gains,
            MarginPct = marginPct,
            SalesCount = currentSnapshot.SalesCount,
            UnitsSold = currentSnapshot.UnitsSold,
            AvgTicket = currentSnapshot.AvgTicket,
            DebtsTotal = debtsTotal,
            DebtRatioPct = debtRatioPct,
            LowStockCount = stockRisk.LowStockCount,
            OutOfStockCount = stockRisk.OutOfStockCount,
            TopCustomer = currentSnapshot.TopCustomer,
            TopProductByQuantity = currentSnapshot.TopProductByQuantity,
            Comparison = comparison,
            Alerts = BuildAlerts(gains, debtRatioPct, stockRisk.LowStockCount, stockRisk.OutOfStockCount)
        };
    }

    public async Task<SalesByPeriodDto> GetSalesByPeriodAsync(string groupBy, DateTime? from = null, DateTime? to = null, int tzOffsetMinutes = 0, CancellationToken ct = default)
    {
        var normalized = NormalizeGroupBy(groupBy);
        var sales = await ApplySalesFilter(_db.Sales.AsNoTracking(), from, to)
            .Select(s => new { s.CreatedAt, s.Total })
            .ToListAsync(ct);

        var points = sales
            .GroupBy(s => GetPeriodStart(s.CreatedAt, normalized, tzOffsetMinutes))
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
                CustomerName = g.Select(s => s.Customer != null ? s.Customer.Name : null).FirstOrDefault() ?? "Unknown",
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
                CustomerName = g.Select(s => s.Customer != null ? s.Customer.Name : null).FirstOrDefault() ?? "Unknown",
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

    private async Task<SalesSnapshot> BuildSalesSnapshotAsync(DateTime? from, DateTime? to, bool includeInsights, CancellationToken ct)
    {
        var sales = await ApplySalesFilter(_db.Sales.AsNoTracking(), from, to)
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .ToListAsync(ct);

        var revenueTotal = sales.Sum(s => s.Total);
        var salesCount = sales.Count;
        var unitsSold = sales.Sum(s => s.Items.Sum(i => i.Quantity));
        var avgTicket = salesCount == 0 ? 0m : revenueTotal / salesCount;

        if (!includeInsights)
        {
            return new SalesSnapshot
            {
                RevenueTotal = revenueTotal,
                SalesCount = salesCount,
                UnitsSold = unitsSold,
                AvgTicket = avgTicket
            };
        }

        CustomerSpendingDto? topCustomer = sales
            .Where(s => s.CustomerId is not null)
            .GroupBy(s => new
            {
                CustomerId = s.CustomerId!.Value,
                CustomerName = s.Customer?.Name ?? "Unknown"
            })
            .Select(g => new CustomerSpendingDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                TotalSpent = g.Sum(s => s.Total)
            })
            .OrderByDescending(x => x.TotalSpent)
            .ThenBy(x => x.CustomerName)
            .FirstOrDefault();

        TopProductDto? topProductByQuantity = null;
        var topProductGroup = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * (x.SpecialPrice ?? x.UnitPrice))
            })
            .OrderByDescending(x => x.Quantity)
            .ThenByDescending(x => x.Revenue)
            .FirstOrDefault();

        if (topProductGroup is not null)
        {
            var productName = await _db.Products
                .AsNoTracking()
                .Where(p => p.Id == topProductGroup.ProductId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(ct);

            topProductByQuantity = new TopProductDto
            {
                ProductId = topProductGroup.ProductId,
                ProductName = productName ?? string.Empty,
                Quantity = topProductGroup.Quantity,
                Revenue = topProductGroup.Revenue
            };
        }

        return new SalesSnapshot
        {
            RevenueTotal = revenueTotal,
            SalesCount = salesCount,
            UnitsSold = unitsSold,
            AvgTicket = avgTicket,
            TopCustomer = topCustomer,
            TopProductByQuantity = topProductByQuantity
        };
    }

    private async Task<decimal> GetCostsTotalAsync(DateTime? from, DateTime? to, CancellationToken ct)
        => await ApplyCostsFilter(_db.Costs.AsNoTracking(), from, to).SumAsync(c => (decimal?)c.Amount, ct) ?? 0m;

    private async Task<decimal> GetDebtsTotalAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var debtQuery = ApplySalesFilterWithDebtOption(_db.Sales.AsNoTracking(), from, to, excludeDebts: false)
            .Where(s => s.IsDebt);

        return await debtQuery.SumAsync(s => (decimal?)s.Total, ct) ?? 0m;
    }

    private async Task<StockRiskSnapshot> GetStockRiskAsync(CancellationToken ct)
    {
        var stocks = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Stock.HasValue)
            .Select(p => p.Stock!.Value)
            .ToListAsync(ct);

        return new StockRiskSnapshot
        {
            LowStockCount = stocks.Count(stock => stock > 0 && stock <= LowStockThreshold),
            OutOfStockCount = stocks.Count(stock => stock <= 0)
        };
    }

    private static IQueryable<Sale> ApplySalesFilter(IQueryable<Sale> query, DateTime? from, DateTime? to)
    {
        if (from is not null)
            query = query.Where(s => s.CreatedAt >= from.Value);

        if (to is not null)
            query = query.Where(s => s.CreatedAt <= to.Value);

        query = query.Where(s => !s.IsDebt);
        return query;
    }

    private static IQueryable<Sale> ApplySalesFilterWithDebtOption(IQueryable<Sale> query, DateTime? from, DateTime? to, bool? excludeDebts)
    {
        if (from is not null)
            query = query.Where(s => s.CreatedAt >= from.Value);

        if (to is not null)
            query = query.Where(s => s.CreatedAt <= to.Value);

        if (excludeDebts == true)
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

    private static DateTime GetPeriodStart(DateTime dt, string groupBy, int tzOffsetMinutes = 0)
    {
        var localDt = dt.ToUniversalTime().AddMinutes(-tzOffsetMinutes);

        DateTime periodStart = groupBy switch
        {
            "day" => new DateTime(localDt.Year, localDt.Month, localDt.Day, 0, 0, 0, DateTimeKind.Unspecified),
            "month" => new DateTime(localDt.Year, localDt.Month, 1, 0, 0, 0, DateTimeKind.Unspecified),
            "week" => StartOfWeekLocal(localDt, DayOfWeek.Monday),
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy))
        };

        return DateTime.SpecifyKind(periodStart.AddMinutes(tzOffsetMinutes), DateTimeKind.Utc);
    }

    private static (DateTime From, DateTime To)? GetPreviousRange(DateTime? from, DateTime? to)
    {
        if (from is null || to is null || to < from)
            return null;

        var duration = to.Value - from.Value;
        var previousTo = from.Value.AddTicks(-1);
        var previousFrom = previousTo - duration;
        return (previousFrom, previousTo);
    }

    private static decimal? CalculateDeltaPct(decimal current, decimal previous)
    {
        if (previous == 0m)
            return current == 0m ? 0m : null;

        return Math.Round(((current - previous) / previous) * 100m, 2);
    }

    private static decimal? CalculateDeltaPct(int current, int previous)
        => CalculateDeltaPct((decimal)current, previous);

    private static IReadOnlyList<DashboardAlertDto> BuildAlerts(decimal gains, decimal debtRatioPct, int lowStockCount, int outOfStockCount)
    {
        var alerts = new List<DashboardAlertDto>();

        if (gains < 0m)
        {
            alerts.Add(new DashboardAlertDto
            {
                Kind = "danger",
                Title = "Negative gains",
                Detail = "Costs are higher than revenue for the selected period."
            });
        }

        if (debtRatioPct >= 20m)
        {
            alerts.Add(new DashboardAlertDto
            {
                Kind = "warning",
                Title = "High debt exposure",
                Detail = "Debt sales represent a significant share of total sales."
            });
        }

        if (outOfStockCount > 0)
        {
            alerts.Add(new DashboardAlertDto
            {
                Kind = "danger",
                Title = "Products out of stock",
                Detail = $"{outOfStockCount} active product(s) have no stock available."
            });
        }

        if (lowStockCount > 0)
        {
            alerts.Add(new DashboardAlertDto
            {
                Kind = "info",
                Title = "Low stock detected",
                Detail = $"{lowStockCount} active product(s) are running low on stock."
            });
        }

        return alerts;
    }

    private static DateTime StartOfWeekLocal(DateTime dt, DayOfWeek startOfWeek)
    {
        var date = dt.Date;
        var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff);
    }

    private sealed class SalesSnapshot
    {
        public decimal RevenueTotal { get; init; }
        public int SalesCount { get; init; }
        public int UnitsSold { get; init; }
        public decimal AvgTicket { get; init; }
        public CustomerSpendingDto? TopCustomer { get; init; }
        public TopProductDto? TopProductByQuantity { get; init; }
    }

    private sealed class StockRiskSnapshot
    {
        public int LowStockCount { get; init; }
        public int OutOfStockCount { get; init; }
    }
}
