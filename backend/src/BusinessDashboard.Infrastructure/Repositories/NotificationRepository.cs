using BusinessDashboard.Domain.Notifications;
using BusinessDashboard.Infrastructure.Persistence;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);

        if (notification is null)
            throw new KeyNotFoundException($"Notification with ID {id} not found.");

        return notification;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.Date)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Notification>> GetUnseenAsync(CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(n => !n.IsSeen)
            .OrderByDescending(n => n.Date)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await GetByIdAsync(id, ct);
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsWithTitleAsync(string title, CancellationToken ct = default)
    {
        return await _context.Notifications
            .AnyAsync(n => n.Title == title, ct);
    }
}
