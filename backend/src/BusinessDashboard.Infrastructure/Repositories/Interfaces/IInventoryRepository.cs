using BusinessDashboard.Domain.Inventory;

namespace BusinessDashboard.Infrastructure.Repositories.Interfaces;

public interface IInventoryRepository
{
    Task AddAsync(InventoryMovement movement, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryMovement>> GetAsync(Guid? productId = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<(IReadOnlyList<InventoryMovement> Items, int Total)> GetPageAsync(
        Guid? productId = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);
}
