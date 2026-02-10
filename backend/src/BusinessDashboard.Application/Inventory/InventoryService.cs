using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

namespace BusinessDashboard.Application.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _products;
    private readonly IInventoryRepository _movements;

    public InventoryService(IProductRepository products, IInventoryRepository movements)
    {
        _products = products;
        _movements = movements;
    }

    public async Task AdjustStockAsync(Guid productId, int delta, CancellationToken ct = default)
    {
        if (delta == 0) return;

        var product = await _products.GetByIdAsync(productId);

        if (product.Stock is null)
            throw new InvalidOperationException("Stock is not tracked for this product.");

        product.AdjustStock(delta);

        var movement = new InventoryMovement(
            productId: productId,
            type: delta > 0 ? InventoryMovementType.In : InventoryMovementType.Out,
            reason: InventoryMovementReason.Adjustment,
            quantity: Math.Abs(delta),
            createdAt: DateTime.UtcNow
        );

        await _products.UpdateAsync(product, new[] { movement }, ct);
    }
}
