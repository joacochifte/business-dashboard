namespace BusinessDashboard.Infrastructure.Inventory;
public class InventoryMovementItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public DateTime CreatedAt { get; init; }
}