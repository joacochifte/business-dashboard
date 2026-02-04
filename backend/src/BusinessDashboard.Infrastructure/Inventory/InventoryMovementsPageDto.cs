namespace BusinessDashboard.Infrastructure.Inventory;

public sealed class InventoryMovementsPageDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public IReadOnlyList<InventoryMovementItemDto> Items { get; init; } = Array.Empty<InventoryMovementItemDto>();
}

