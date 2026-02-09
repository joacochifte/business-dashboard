namespace BusinessDashboard.Api.Controllers;
using BusinessDashboard.Application.Costs;
using BusinessDashboard.Infrastructure.Costs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("costs")]
public class CostsController : ControllerBase
{
    private readonly ICostService _costService;

    public CostsController(ICostService costService)
    {
        _costService = costService;
    }

    [HttpPost]
    public async Task<IActionResult> AddCost([FromBody] CostCreationDto costDto, CancellationToken cancellationToken)
    {
        var id = await _costService.AddCostAsync(costDto, cancellationToken);
        return CreatedAtAction(nameof(GetCostById), new { id }, null);
    }

    [HttpGet]
    public async Task<IActionResult> GetCosts([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
    {
        if (startDate is not null && endDate is not null && startDate > endDate)
            return BadRequest("'startDate' must be <= 'endDate'.");

        var costs = await _costService.GetCostsAsync(startDate, endDate, cancellationToken);
        return Ok(costs);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCostById(Guid id, CancellationToken cancellationToken)
    {
        var cost = await _costService.GetCostByIdAsync(id, cancellationToken);
        if (cost is null)
            return NotFound();

        var dto = new CostCreationDto
        {
            Id = cost.Id,
            Name = cost.Name,
            Amount = cost.Amount,
            DateIncurred = cost.DateIncurred,
            Description = cost.Description
        };

        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCost(Guid id, [FromBody] CostCreationDto costDto, CancellationToken cancellationToken)
    {
        await _costService.UpdateCostAsync(id, costDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCost(Guid id, CancellationToken cancellationToken)
    {
        await _costService.DeleteCostAsync(id, cancellationToken);
        return NoContent();
    }
}
