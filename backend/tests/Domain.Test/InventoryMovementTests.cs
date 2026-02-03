using BusinessDashboard.Domain.Inventory;

namespace Domain.Test.Inventory;

[TestClass]
public class InventoryMovementTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithEmptyProductId_ShouldThrow()
    {
        _ = new InventoryMovement(
            Guid.Empty,
            InventoryMovementType.In,
            InventoryMovementReason.Purchase,
            quantity: 10);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_WithZeroQuantity_ShouldThrow()
    {
        _ = new InventoryMovement(
            Guid.NewGuid(),
            InventoryMovementType.In,
            InventoryMovementReason.Purchase,
            quantity: 0);
    }

    [TestMethod]
    public void Constructor_WithValidData_ShouldCreateInstance()
    {
        var movement = new InventoryMovement(
            Guid.NewGuid(),
            InventoryMovementType.Out,
            InventoryMovementReason.Sale,
            quantity: 3);

        Assert.IsNotNull(movement);
        Assert.AreEqual(3, movement.Quantity);
        Assert.AreEqual(InventoryMovementType.Out, movement.Type);
        Assert.AreEqual(InventoryMovementReason.Sale, movement.Reason);
    }
}
