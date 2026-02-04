using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Inventory;

public class InventoryMovement : Entity
{
    public Guid ProductId { get; private set; }
    public InventoryMovementType Type { get; private set; }
    public InventoryMovementReason Reason { get; private set; }
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private InventoryMovement() { }

    public InventoryMovement(
        Guid productId,
        InventoryMovementType type,
        InventoryMovementReason reason,
        int quantity, DateTime createdAt)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ProductId = productId;
        Type = type;
        Reason = reason;
        Quantity = quantity;
        CreatedAt = createdAt;
    }
}
