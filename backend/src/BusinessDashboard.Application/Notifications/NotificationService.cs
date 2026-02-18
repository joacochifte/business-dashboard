using BusinessDashboard.Domain.Notifications;
using BusinessDashboard.Infrastructure.Notifications;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;

namespace BusinessDashboard.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateNotificationAsync(NotificationCreationDto request, CancellationToken ct = default)
    {
        var notification = new Notification(request.Title, request.Date);
        await _repo.AddAsync(notification, ct);
        return notification.Id;
    }

    public async Task DeleteNotificationAsync(Guid notificationId, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(notificationId, ct);
    }

    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(CancellationToken ct = default)
    {
        var notifications = await _repo.GetAllAsync(ct);
        return notifications.Select(MapToDto);
    }

    public async Task<IEnumerable<NotificationDto>> GetUnseenNotificationsAsync(CancellationToken ct = default)
    {
        var notifications = await _repo.GetUnseenAsync(ct);
        return notifications.Select(MapToDto);
    }

    public async Task<NotificationDto> GetNotificationByIdAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _repo.GetByIdAsync(notificationId, ct);
        return MapToDto(notification);
    }

    public async Task UpdateNotificationAsync(Guid notificationId, NotificationUpdateDto request, CancellationToken ct = default)
    {
        var notification = await _repo.GetByIdAsync(notificationId, ct);
        notification.SetTitle(request.Title);
        notification.SetDate(request.Date);
        await _repo.UpdateAsync(notification, ct);
    }

    public async Task MarkAsSeenAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _repo.GetByIdAsync(notificationId, ct);
        notification.MarkAsSeen();
        await _repo.UpdateAsync(notification, ct);
    }

    public async Task MarkAllAsSeenAsync(CancellationToken ct = default)
    {
        var unseen = await _repo.GetUnseenAsync(ct);
        foreach (var notification in unseen)
            notification.MarkAsSeen();

        foreach (var notification in unseen)
            await _repo.UpdateAsync(notification, ct);
    }

    private static NotificationDto MapToDto(Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Date = notification.Date,
        IsSeen = notification.IsSeen
    };
}
