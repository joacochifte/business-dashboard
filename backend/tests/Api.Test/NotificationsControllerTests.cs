using BusinessDashboard.Application.Notifications;
using BusinessDashboard.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class NotificationsControllerTests
{
    private FakeNotificationService _service = null!;
    private NotificationsController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeNotificationService();
        _controller = new NotificationsController(_service);
    }

    [TestMethod]
    public async Task GetNotifications_ShouldReturnOkWithNotifications()
    {
        var expected = new List<NotificationDto>
        {
            new() { Id = Guid.NewGuid(), Title = "A", Date = DateTime.UtcNow, IsSeen = false },
            new() { Id = Guid.NewGuid(), Title = "B", Date = DateTime.UtcNow, IsSeen = true }
        };
        _service.GetAllResult = expected;

        var result = await _controller.GetNotifications();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<NotificationDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count());
    }

    [TestMethod]
    public async Task GetUnseen_ShouldReturnOkWithUnseenNotifications()
    {
        var expected = new List<NotificationDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Sin ver", Date = DateTime.UtcNow, IsSeen = false }
        };
        _service.GetUnseenResult = expected;

        var result = await _controller.GetUnseen();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<NotificationDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count());
    }

    [TestMethod]
    public async Task GetNotificationById_ShouldReturnOkWithNotification()
    {
        var id = Guid.NewGuid();
        var expected = new NotificationDto { Id = id, Title = "A", Date = DateTime.UtcNow, IsSeen = false };
        _service.GetByIdResult = expected;

        var result = await _controller.GetNotificationById(id);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as NotificationDto;
        Assert.IsNotNull(value);
        Assert.AreEqual(id, value.Id);
    }

    [TestMethod]
    public async Task CreateNotification_ShouldReturnCreatedAtAction()
    {
        var id = Guid.NewGuid();
        _service.CreateResult = id;
        var request = new NotificationCreationDto { Title = "Recordatorio", Date = DateTime.UtcNow };

        var result = await _controller.CreateNotification(request);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(nameof(NotificationsController.GetNotificationById), created.ActionName);
        Assert.IsNotNull(created.RouteValues);
        Assert.AreEqual(id, created.RouteValues["id"]);
    }

    [TestMethod]
    public async Task UpdateNotification_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var request = new NotificationUpdateDto { Title = "Actualizado", Date = DateTime.UtcNow };

        var result = await _controller.UpdateNotification(id, request);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastUpdatedId);
    }

    [TestMethod]
    public async Task MarkAsSeen_ShouldReturnOk()
    {
        var id = Guid.NewGuid();

        var result = await _controller.MarkAsSeen(id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastSeenId);
    }

    [TestMethod]
    public async Task MarkAllAsSeen_ShouldReturnOk()
    {
        var result = await _controller.MarkAllAsSeen();

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsTrue(_service.MarkAllSeenCalled);
    }

    [TestMethod]
    public async Task DeleteNotification_ShouldReturnOk()
    {
        var id = Guid.NewGuid();

        var result = await _controller.DeleteNotification(id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastDeletedId);
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public IEnumerable<NotificationDto> GetAllResult { get; set; } = Array.Empty<NotificationDto>();
        public IEnumerable<NotificationDto> GetUnseenResult { get; set; } = Array.Empty<NotificationDto>();
        public NotificationDto GetByIdResult { get; set; } = new();
        public Guid CreateResult { get; set; } = Guid.NewGuid();
        public Guid? LastUpdatedId { get; private set; }
        public Guid? LastDeletedId { get; private set; }
        public Guid? LastSeenId { get; private set; }
        public bool MarkAllSeenCalled { get; private set; }

        public Task<Guid> CreateNotificationAsync(NotificationCreationDto request, CancellationToken ct = default)
            => Task.FromResult(CreateResult);

        public Task DeleteNotificationAsync(Guid notificationId, CancellationToken ct = default)
        {
            LastDeletedId = notificationId;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(CancellationToken ct = default)
            => Task.FromResult(GetAllResult);

        public Task<IEnumerable<NotificationDto>> GetUnseenNotificationsAsync(CancellationToken ct = default)
            => Task.FromResult(GetUnseenResult);

        public Task<NotificationDto> GetNotificationByIdAsync(Guid notificationId, CancellationToken ct = default)
            => Task.FromResult(GetByIdResult);

        public Task UpdateNotificationAsync(Guid notificationId, NotificationUpdateDto request, CancellationToken ct = default)
        {
            LastUpdatedId = notificationId;
            return Task.CompletedTask;
        }

        public Task MarkAsSeenAsync(Guid notificationId, CancellationToken ct = default)
        {
            LastSeenId = notificationId;
            return Task.CompletedTask;
        }

        public Task MarkAllAsSeenAsync(CancellationToken ct = default)
        {
            MarkAllSeenCalled = true;
            return Task.CompletedTask;
        }
    }
}
