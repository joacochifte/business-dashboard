using BusinessDashboard.Domain.Notifications;

namespace Domain.Test.Notifications;

[TestClass]
public class NotificationTests
{
    private Notification _notification = null!;

    [TestInitialize]
    public void SetUp()
    {
        _notification = new Notification("Llamar al proveedor", new DateTime(2026, 3, 1));
    }

    [TestMethod]
    public void Constructor_ShouldCreateInstance()
    {
        Assert.IsNotNull(_notification);
        Assert.IsInstanceOfType(_notification, typeof(Notification));
    }

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual("Llamar al proveedor", _notification.Title);
        Assert.AreEqual(new DateTime(2026, 3, 1), _notification.Date);
        Assert.IsFalse(_notification.IsSeen);
    }

    // ---------- Title ----------

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithNullTitle_ShouldThrowArgumentException()
    {
        _ = new Notification(null!, DateTime.UtcNow);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithWhitespaceTitle_ShouldThrowArgumentException()
    {
        _ = new Notification("   ", DateTime.UtcNow);
    }

    [TestMethod]
    public void SetTitle_WithValidValue_ShouldUpdateTitle()
    {
        _notification.SetTitle("Nuevo título");
        Assert.AreEqual("Nuevo título", _notification.Title);
    }

    [TestMethod]
    public void SetTitle_ShouldTrimWhitespace()
    {
        _notification.SetTitle("  Recordatorio  ");
        Assert.AreEqual("Recordatorio", _notification.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SetTitle_WithEmpty_ShouldThrowArgumentException()
    {
        _notification.SetTitle("");
    }

    // ---------- Date ----------

    [TestMethod]
    public void SetDate_ShouldUpdateDate()
    {
        var date = new DateTime(2026, 6, 15);
        _notification.SetDate(date);
        Assert.AreEqual(date, _notification.Date);
    }

    // ---------- IsSeen ----------

    [TestMethod]
    public void MarkAsSeen_ShouldSetIsSeenToTrue()
    {
        _notification.MarkAsSeen();
        Assert.IsTrue(_notification.IsSeen);
    }

    [TestMethod]
    public void MarkAsUnseen_ShouldSetIsSeenToFalse()
    {
        _notification.MarkAsSeen();
        _notification.MarkAsUnseen();
        Assert.IsFalse(_notification.IsSeen);
    }
}
