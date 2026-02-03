using BusinessDashboard.Domain.Sales;

namespace Domain.Test.Sales;

[TestClass]
public class SaleItemTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithEmptyProductId_ShouldThrow()
    {
        _ = new SaleItem(Guid.Empty, quantity: 1, unitPrice: 100m);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_WithZeroQuantity_ShouldThrow()
    {
        _ = new SaleItem(Guid.NewGuid(), quantity: 0, unitPrice: 100m);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_WithNegativeUnitPrice_ShouldThrow()
    {
        _ = new SaleItem(Guid.NewGuid(), quantity: 1, unitPrice: -10m);
    }

    [TestMethod]
    public void LineTotal_ShouldBeQuantityTimesUnitPrice()
    {
        var item = new SaleItem(Guid.NewGuid(), quantity: 2, unitPrice: 150m);

        Assert.AreEqual(300m, item.LineTotal);
    }
}
