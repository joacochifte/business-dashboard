namespace BusinessDashboard.Application.Inventory;

public interface IInventoryService
{
    Task AdjustStockAsync(Guid productId, int delta, CancellationToken ct = default);
}
