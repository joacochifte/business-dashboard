using BusinessDashboard.Application.Dashboard;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("dashboard")]
public class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        var result = await dashboard.GetSummaryAsync(from, to, ct);
        return Ok(result);
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        var result = await dashboard.GetOverviewAsync(from, to, ct);
        return Ok(result);
    }

    [HttpGet("performance-series")]
    public async Task<IActionResult> GetPerformanceSeries(
        [FromQuery] string groupBy = "day",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int tzOffsetMinutes = 0,
        [FromQuery] string? compareYears = null,
        [FromQuery] string? forecastModel = null,
        [FromQuery] bool includeForecast = false,
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        try
        {
            var compareYearOffsets = ParseCompareYears(compareYears);

            var result = await dashboard.GetPerformanceSeriesAsync(
                groupBy,
                from,
                to,
                tzOffsetMinutes,
                compareYearOffsets,
                forecastModel,
                includeForecast,
                ct);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("sales-by-period")]
    public async Task<IActionResult> GetSalesByPeriod(
        [FromQuery] string groupBy = "day",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int tzOffsetMinutes = 0,
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        try
        {
            var result = await dashboard.GetSalesByPeriodAsync(groupBy, from, to, tzOffsetMinutes, ct);
            return Ok(result);
        }
        catch (ArgumentException ex) when (ex.ParamName == "groupBy")
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string sortBy = "revenue",
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        var result = await dashboard.GetTopProductsAsync(limit, from, to, sortBy, ct);
        return Ok(result);
    }

    [HttpGet("sales-by-customer")]
    public async Task<IActionResult> GetSalesByCustomer(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool? excludeDebts = true,
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        var result = await dashboard.GetSalesByCustomerAsync(limit, from, to, excludeDebts, ct);
        return Ok(result);
    }

    [HttpGet("spending-by-customer")]
    public async Task<IActionResult> GetSpendingByCustomer(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool? excludeDebts = true,
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        var result = await dashboard.GetSpendingByCustomerAsync(limit, from, to, excludeDebts, ct);
        return Ok(result);
    }

    private static IReadOnlyList<int> ParseCompareYears(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<int>();

        var values = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v => int.TryParse(v, out var yearOffset) ? yearOffset : throw new ArgumentException("compareYears must be a comma-separated list of integers."))
            .ToArray();

        if (values.Any(v => v <= 0))
            throw new ArgumentException("compareYears values must be positive integers.");

        return values;
    }
}
