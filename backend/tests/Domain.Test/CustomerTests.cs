using BusinessDashboard.Domain.Customers;

namespace Domain.Test.Customers;

[TestClass]
public class CustomerTests
{
    private Customer _customer = null!;

    [TestInitialize]
    public void SetUp()
    {
        _customer = new Customer(name: "Juan Pérez", email: "juan@mail.com", phone: "1234567890", birthDate: new DateTime(1990, 5, 15));
    }

    [TestMethod]
    public void Constructor_ShouldCreateInstance()
    {
        Assert.IsNotNull(_customer);
        Assert.IsInstanceOfType(_customer, typeof(Customer));
    }

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual("Juan Pérez", _customer.Name);
        Assert.AreEqual("juan@mail.com", _customer.Email);
        Assert.AreEqual("1234567890", _customer.Phone);
        Assert.AreEqual(new DateTime(1990, 5, 15), _customer.BirthDate);
        Assert.IsTrue(_customer.IsActive);
        Assert.AreEqual(0, _customer.TotalPurchases);
        Assert.AreEqual(0m, _customer.TotalLifetimeValue);
    }

    // ---------- Name ----------

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithNullName_ShouldThrowArgumentException()
    {
        _ = new Customer(name: null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithWhitespaceName_ShouldThrowArgumentException()
    {
        _ = new Customer(name: "   ");
    }

    [TestMethod]
    public void SetName_WithValidValue_ShouldUpdateName()
    {
        _customer.SetName("María García");
        Assert.AreEqual("María García", _customer.Name);
    }

    [TestMethod]
    public void SetName_ShouldTrimWhitespace()
    {
        _customer.SetName("  Pedro  ");
        Assert.AreEqual("Pedro", _customer.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetName_WithEmpty_ShouldThrowArgumentException()
    {
        _customer.SetName("");
    }

    // ---------- Email ----------

    [TestMethod]
    public void SetEmail_WithValidEmail_ShouldUpdateEmail()
    {
        _customer.SetEmail("nuevo@mail.com");
        Assert.AreEqual("nuevo@mail.com", _customer.Email);
    }

    [TestMethod]
    public void SetEmail_WithNull_ShouldSetNull()
    {
        _customer.SetEmail(null);
        Assert.IsNull(_customer.Email);
    }

    [TestMethod]
    public void SetEmail_WithEmpty_ShouldSetNull()
    {
        _customer.SetEmail("");
        Assert.IsNull(_customer.Email);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetEmail_WithInvalidFormat_ShouldThrowArgumentException()
    {
        _customer.SetEmail("no-es-un-email");
    }

    // ---------- Phone ----------

    [TestMethod]
    public void SetPhone_WithValidValue_ShouldUpdatePhone()
    {
        _customer.SetPhone("9876543210");
        Assert.AreEqual("9876543210", _customer.Phone);
    }

    [TestMethod]
    public void SetPhone_WithNull_ShouldSetNull()
    {
        _customer.SetPhone(null);
        Assert.IsNull(_customer.Phone);
    }

    // ---------- BirthDate ----------

    [TestMethod]
    public void SetBirthDate_WithValidDate_ShouldUpdateBirthDate()
    {
        var date = new DateTime(1985, 3, 20);
        _customer.SetBirthDate(date);
        Assert.AreEqual(date, _customer.BirthDate);
    }

    [TestMethod]
    public void SetBirthDate_WithNull_ShouldSetNull()
    {
        _customer.SetBirthDate(null);
        Assert.IsNull(_customer.BirthDate);
    }

    // ---------- IsActive ----------

    [TestMethod]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        _customer.Deactivate();
        Assert.IsFalse(_customer.IsActive);
    }

    [TestMethod]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        _customer.Deactivate();
        _customer.Activate();
        Assert.IsTrue(_customer.IsActive);
    }

    // ---------- UpdateLastPurchaseDate ----------

    [TestMethod]
    public void UpdateLastPurchaseDate_ShouldUpdateStats()
    {
        var date = new DateTime(2026, 2, 18);
        _customer.UpdateLastPurchaseDate(date, 500m);

        Assert.AreEqual(date, _customer.LastPurchaseDate);
        Assert.AreEqual(1, _customer.TotalPurchases);
        Assert.AreEqual(500m, _customer.TotalLifetimeValue);
    }

    [TestMethod]
    public void UpdateLastPurchaseDate_CalledTwice_ShouldAccumulate()
    {
        _customer.UpdateLastPurchaseDate(new DateTime(2026, 1, 1), 200m);
        _customer.UpdateLastPurchaseDate(new DateTime(2026, 2, 1), 300m);

        Assert.AreEqual(2, _customer.TotalPurchases);
        Assert.AreEqual(500m, _customer.TotalLifetimeValue);
        Assert.AreEqual(new DateTime(2026, 2, 1), _customer.LastPurchaseDate);
    }
}
