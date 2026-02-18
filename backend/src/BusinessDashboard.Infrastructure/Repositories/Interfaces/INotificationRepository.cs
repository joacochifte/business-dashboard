using BusinessDashboard.Domain.Notifications;

namespace BusinessDashboard.Infrastructure.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<Notification> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Notification>> GetUnseenAsync(CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task UpdateAsync(Notification notification, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsWithTitleAsync(string title, CancellationToken ct = default);
}
