using BusinessDashboard.Infrastructure.Notifications;

namespace BusinessDashboard.Application.Notifications;

public interface INotificationService
{
    Task<Guid> CreateNotificationAsync(NotificationCreationDto request, CancellationToken ct = default);
    Task DeleteNotificationAsync(Guid notificationId, CancellationToken ct = default);
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(CancellationToken ct = default);
    Task<IEnumerable<NotificationDto>> GetUnseenNotificationsAsync(CancellationToken ct = default);
    Task<NotificationDto> GetNotificationByIdAsync(Guid notificationId, CancellationToken ct = default);
    Task UpdateNotificationAsync(Guid notificationId, NotificationUpdateDto request, CancellationToken ct = default);
    Task MarkAsSeenAsync(Guid notificationId, CancellationToken ct = default);
    Task MarkAllAsSeenAsync(CancellationToken ct = default);
}
