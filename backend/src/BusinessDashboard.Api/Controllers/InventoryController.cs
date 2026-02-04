using BusinessDashboard.Application.Inventory;
using BusinessDashboard.Infrastructure.Inventory;
using Microsoft.AspNetCore.Mvc;
using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

[ApiController]
[Route("inventory")]
public class InventoryController(IInventoryService inventory, IInventoryRepository repo) : ControllerBase
{
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] InventoryMovementDto request, CancellationToken ct)
    {
        await inventory.AdjustStockAsync(request.ProductId, request.Delta, ct);
        return Ok();
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(CancellationToken ct,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50
        )
    {
        if (productId == Guid.Empty)
            return BadRequest("productId cannot be empty.");

        if (from is not null && to is not null && from > to)
            return BadRequest("'from' must be <= 'to'.");

        if (page < 1)
            return BadRequest("page must be >= 1.");

        if (pageSize is < 1 or > 200)
            return BadRequest("pageSize must be between 1 and 200.");

        var (movements, total) = await repo.GetPageAsync(productId, from, to, page, pageSize, ct);
        var items = movements.Select(m => new InventoryMovementItemDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            Type = Enum.GetName(typeof(InventoryMovementType), m.Type) ?? m.Type.ToString(),
            Reason = Enum.GetName(typeof(InventoryMovementReason), m.Reason) ?? m.Reason.ToString(),
            Quantity = m.Quantity,
            CreatedAt = m.CreatedAt
        }).ToList();

        return Ok(new InventoryMovementsPageDto
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }
}
