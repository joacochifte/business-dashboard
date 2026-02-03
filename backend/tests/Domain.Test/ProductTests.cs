using BusinessDashboard.Domain.Products;

namespace Domain.Test.Products;

[TestClass]
public class ProductTests
{
    private Product _product = null!;

    [TestInitialize]
    public void SetUp()
    {
        _product = new Product(name: "Coca Cola 600ml", price: 100m, initialStock: 10, description: "Bebida");
    }

    [TestMethod]
    public void Constructor_ShouldCreateInstance()
    {
        Assert.IsNotNull(_product);
        Assert.IsInstanceOfType(_product, typeof(Product));
    }

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual("Coca Cola 600ml", _product.Name);
        Assert.AreEqual("Bebida", _product.Description);
        Assert.AreEqual(100m, _product.Price);
        Assert.AreEqual(10, _product.Stock);
        Assert.IsTrue(_product.IsActive);
    }

    // ---------- Name ----------

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithNullName_ShouldThrowArgumentException()
    {
        _ = new Product(name: null!, price: 100m);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithWhitespaceName_ShouldThrowArgumentException()
    {
        _ = new Product(name: "   ", price: 100m);
    }

    [TestMethod]
    public void SetName_WithValidValue_ShouldUpdateName()
    {
        _product.SetName("Pepsi 600ml");
        Assert.AreEqual("Pepsi 600ml", _product.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetName_WithNull_ShouldThrowArgumentException()
    {
        _product.SetName(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetName_WithEmpty_ShouldThrowArgumentException()
    {
        _product.SetName("");
    }

    // ---------- Price ----------

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_WithZeroPrice_ShouldThrowArgumentOutOfRangeException()
    {
        _ = new Product(name: "Producto", price: 0m);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentOutOfRangeException()
    {
        _ = new Product(name: "Producto", price: -1m);
    }

    [TestMethod]
    public void SetPrice_WithValidValue_ShouldUpdatePrice()
    {
        _product.SetPrice(250m);
        Assert.AreEqual(250m, _product.Price);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetPrice_WithZero_ShouldThrowArgumentOutOfRangeException()
    {
        _product.SetPrice(0m);
    }

    // ---------- Stock ----------

    [TestMethod]
    public void Constructor_WithNullInitialStock_ShouldSetStockToNull()
    {
        var product = new Product(name: "Servicio", price: 50m, initialStock: null);
        Assert.IsNull(product.Stock);
    }

    [TestMethod]
    public void AdjustStock_WithPositiveDelta_ShouldIncreaseStock()
    {
        _product.AdjustStock(+5);
        Assert.AreEqual(15, _product.Stock);
    }

    [TestMethod]
    public void AdjustStock_WithNegativeDeltaWithinStock_ShouldDecreaseStock()
    {
        _product.AdjustStock(-3);
        Assert.AreEqual(7, _product.Stock);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AdjustStock_ResultingNegativeStock_ShouldThrowInvalidOperationException()
    {
        _product.AdjustStock(-999);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AdjustStock_WhenStockNotTracked_ShouldThrowInvalidOperationException()
    {
        var product = new Product(name: "Servicio", price: 50m, initialStock: null);
        product.AdjustStock(1);
    }

    // ---------- Active ----------

    [TestMethod]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        _product.Deactivate();
        Assert.IsFalse(_product.IsActive);
    }
}
