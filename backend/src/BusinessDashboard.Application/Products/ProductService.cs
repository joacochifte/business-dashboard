using BusinessDashboard.Domain.Products;
using BusinessDashboard.Infrastructure.Products;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Domain.Inventory;
namespace BusinessDashboard.Application.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

	public async Task<Guid> CreateProductAsync(ProductCreationDto request, CancellationToken ct = default)
	{
		var product = new Product(
			name: request.Name,
			price: request.Price,
			initialStock: request.InitialStock,
			description: request.Description
		);

		var movements = GetMovementsForStock(product.Id, oldStock: null, newStock: product.Stock, createdAt: DateTime.UtcNow);

		await _repo.AddAsync(product, movements, ct);
		return product.Id;
	}

	public async Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
	{
		var product = await _repo.GetByIdAsync(productId);
		var movements = GetMovementsForStock(product.Id, oldStock: product.Stock, newStock: null, createdAt: DateTime.UtcNow);
		await _repo.DeleteAsync(productId, movements, ct);
	}

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default)
    {
        var products = await _repo.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            IsActive = p.IsActive
        });
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(productId);
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        };
    }

	public async Task UpdateProductAsync(Guid productId, ProductUpdateDto request, CancellationToken ct = default)
	{
		var product = await _repo.GetByIdAsync(productId);
		var oldStock = product.Stock;
		product.Update(
			name: request.Name,
			description: request.Description,
			price: request.Price,
			stock: request.Stock,
			isActive: request.IsActive
		);

		var movements = GetMovementsForStock(product.Id, oldStock: oldStock, newStock: product.Stock, createdAt: DateTime.UtcNow);
		await _repo.UpdateAsync(product, movements, ct);
	}

	private static IReadOnlyList<InventoryMovement> GetMovementsForStock(
		Guid productId,
		int? oldStock,
		int? newStock,
		DateTime createdAt)
	{
		var movements = new List<InventoryMovement>();

		// Untracked -> untracked.
		if (oldStock is null && newStock is null)
			return movements;

		// Tracked -> untracked: treat as removing tracked stock from the system history.
		if (oldStock is not null && newStock is null)
		{
			if (oldStock.Value <= 0) return movements;

			movements.Add(new InventoryMovement(
				productId: productId,
				type: InventoryMovementType.Out,
				reason: InventoryMovementReason.Adjustment,
				quantity: oldStock.Value,
				createdAt: createdAt
			));
			return movements;
		}

		// Untracked -> tracked: treat as initial stock entry.
		if (oldStock is null && newStock is not null)
		{
			if (newStock.Value <= 0) return movements;

			movements.Add(new InventoryMovement(
				productId: productId,
				type: InventoryMovementType.In,
				reason: InventoryMovementReason.Adjustment,
				quantity: newStock.Value,
				createdAt: createdAt
			));
			return movements;
		}

		// Tracked -> tracked: record the delta.
		var delta = newStock!.Value - oldStock!.Value;
		if (delta == 0) return movements;

		movements.Add(new InventoryMovement(
			productId: productId,
			type: delta > 0 ? InventoryMovementType.In : InventoryMovementType.Out,
			reason: InventoryMovementReason.Adjustment,
			quantity: Math.Abs(delta),
			createdAt: createdAt
		));

		return movements;
	}
}
