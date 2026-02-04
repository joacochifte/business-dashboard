using Microsoft.AspNetCore.Mvc;
using BusinessDashboard.Application.Products;
using BusinessDashboard.Infrastructure.Products;
[ApiController]
[Route("products")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await productService.GetAllProductsAsync();
        return Ok(products);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await productService.GetProductByIdAsync(id);
        return Ok(product);
    }
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreationDto request)
    {  
        return CreatedAtAction(
            nameof(GetProductById),
            new { id = await productService.CreateProductAsync(request) },
            null
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDto request)
    {
        await productService.UpdateProductAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await productService.DeleteProductAsync(id);
        return Ok();
    }
}