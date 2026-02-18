using BusinessDashboard.Application.Notifications;
using BusinessDashboard.Domain.Notifications;
using BusinessDashboard.Infrastructure.Notifications;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Moq;

namespace Application.Test.Notifications;

[TestClass]
public class NotificationServiceTests
{
    private Mock<INotificationRepository> _repo = null!;
    private NotificationService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repo = new Mock<INotificationRepository>(MockBehavior.Strict);
        _service = new NotificationService(_repo.Object);
    }

    [TestMethod]
    public async Task CreateNotificationAsync_ShouldCreateAndReturnId()
    {
        var request = new NotificationCreationDto
        {
            Title = "Llamar al proveedor",
            Date = new DateTime(2026, 3, 1)
        };

        Notification? captured = null;
        _repo
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .Returns(Task.CompletedTask);

        var id = await _service.CreateNotificationAsync(request);

        Assert.IsNotNull(captured);
        Assert.AreEqual(request.Title, captured.Title);
        Assert.AreEqual(request.Date, captured.Date);
        Assert.IsFalse(captured.IsSeen);
        Assert.AreEqual(captured.Id, id);

        _repo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAllNotificationsAsync_ShouldMapToDto()
    {
        var notifications = new List<Notification>
        {
            new Notification("Titulo A", new DateTime(2026, 1, 1)),
            new Notification("Titulo B", new DateTime(2026, 2, 1))
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(notifications);

        var result = (await _service.GetAllNotificationsAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Titulo A", result[0].Title);
        Assert.AreEqual("Titulo B", result[1].Title);

        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetUnseenNotificationsAsync_ShouldReturnOnlyUnseen()
    {
        var unseen = new List<Notification>
        {
            new Notification("Sin ver", new DateTime(2026, 3, 1))
        };

        _repo.Setup(r => r.GetUnseenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(unseen);

        var result = (await _service.GetUnseenNotificationsAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.IsFalse(result[0].IsSeen);

        _repo.Verify(r => r.GetUnseenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationByIdAsync_ShouldMapToDto()
    {
        var id = Guid.NewGuid();
        var notification = new Notification("Recordatorio", new DateTime(2026, 3, 1));

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        var dto = await _service.GetNotificationByIdAsync(id);

        Assert.AreEqual(notification.Title, dto.Title);
        Assert.AreEqual(notification.Date, dto.Date);
        Assert.AreEqual(notification.Id, dto.Id);

        _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteNotificationAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.DeleteNotificationAsync(id);

        _repo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateNotificationAsync_ShouldUpdateAndPersist()
    {
        var id = Guid.NewGuid();
        var notification = new Notification("Titulo viejo", new DateTime(2026, 1, 1));
        var request = new NotificationUpdateDto
        {
            Title = "Titulo nuevo",
            Date = new DateTime(2026, 6, 1)
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
        _repo.Setup(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.UpdateNotificationAsync(id, request);

        Assert.AreEqual("Titulo nuevo", notification.Title);
        Assert.AreEqual(new DateTime(2026, 6, 1), notification.Date);

        _repo.Verify(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task MarkAsSeenAsync_ShouldSetIsSeenTrue()
    {
        var id = Guid.NewGuid();
        var notification = new Notification("Recordatorio", new DateTime(2026, 3, 1));

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
        _repo.Setup(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.MarkAsSeenAsync(id);

        Assert.IsTrue(notification.IsSeen);
        _repo.Verify(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task MarkAllAsSeenAsync_ShouldMarkAllUnseen()
    {
        var notifications = new List<Notification>
        {
            new Notification("A", new DateTime(2026, 1, 1)),
            new Notification("B", new DateTime(2026, 2, 1))
        };

        _repo.Setup(r => r.GetUnseenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(notifications);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.MarkAllAsSeenAsync();

        Assert.IsTrue(notifications.All(n => n.IsSeen));
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
