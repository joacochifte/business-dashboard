namespace BusinessDashboard.Infrastructure.Inventory;

public sealed class InventoryMovementDto
{
    public Guid ProductId { get; init; }
    public int Delta { get; init; }
}