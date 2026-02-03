using BusinessDashboard.Domain.Sales;

namespace Domain.Test.Sales;

[TestClass]
public class SaleTests
{
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Constructor_WithNoItems_ShouldThrow()
    {
        _ = new Sale(new List<SaleItem>());
    }

    [TestMethod]
    public void Constructor_WithItems_ShouldCalculateTotal()
    {
        var items = new List<SaleItem>
        {
            new SaleItem(Guid.NewGuid(), 2, 100m),
            new SaleItem(Guid.NewGuid(), 1, 50m)
        };

        var sale = new Sale(items);

        Assert.AreEqual(250m, sale.Total);
        Assert.AreEqual(2, sale.Items.Count);
    }

    [TestMethod]
    public void AddItem_ShouldUpdateTotal()
    {
        var initialItems = new List<SaleItem>
        {
            new SaleItem(Guid.NewGuid(), 1, 100m)
        };

        var sale = new Sale(initialItems);

        sale.AddItem(new SaleItem(Guid.NewGuid(), 3, 50m));

        Assert.AreEqual(250m, sale.Total);
        Assert.AreEqual(2, sale.Items.Count);
    }

    [TestMethod]
    public void Items_ShouldBeReadOnly()
    {
        var items = new List<SaleItem>
        {
            new SaleItem(Guid.NewGuid(), 1, 100m)
        };

        var sale = new Sale(items);

        Assert.IsTrue(((ICollection<SaleItem>)sale.Items).IsReadOnly);
    }
}
