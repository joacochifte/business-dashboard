using Microsoft.AspNetCore.Mvc;
using BusinessDashboard.Application.Customers;
using BusinessDashboard.Infrastructure.Customers;

[ApiController]
[Route("customers")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var customer = await customerService.GetCustomerByIdAsync(id);
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreationDto request)
    {
        return CreatedAtAction(
            nameof(GetCustomerById),
            new { id = await customerService.CreateCustomerAsync(request) },
            null
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto request)
    {
        await customerService.UpdateCustomerAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await customerService.DeleteCustomerAsync(id);
        return Ok();
    }
}
