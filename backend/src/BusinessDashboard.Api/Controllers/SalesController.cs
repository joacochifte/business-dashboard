using Microsoft.AspNetCore.Mvc;
using BusinessDashboard.Application.Sales;
using BusinessDashboard.Infrastructure.Sales;

[ApiController]
[Route("sales")]
public class SalesController(ISalesService salesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSales([FromQuery] bool? isDebt = null)
    {
        var sales = await salesService.GetAllSalesAsync(isDebt);
        return Ok(sales);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSaleById(Guid id)
    {
        var sale = await salesService.GetSaleByIdAsync(id);
        return Ok(sale);
    }
    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] SaleCreationDto request)
    {  
        return CreatedAtAction(
            nameof(GetSaleById),
            new { id = await salesService.CreateSaleAsync(request) },
            null
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSale(Guid id, [FromBody] SaleUpdateDto request)
    {
        await salesService.UpdateSaleAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSale(Guid id)
    {
        await salesService.DeleteSaleAsync(id);
        return Ok();
    }
}