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

    [HttpGet("sales-by-period")]
    public async Task<IActionResult> GetSalesByPeriod(
        [FromQuery] string groupBy = "day",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        try
        {
            var result = await dashboard.GetSalesByPeriodAsync(groupBy, from, to, ct);
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
        CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        var result = await dashboard.GetTopProductsAsync(limit, from, to, ct);
        return Ok(result);
    }
}
