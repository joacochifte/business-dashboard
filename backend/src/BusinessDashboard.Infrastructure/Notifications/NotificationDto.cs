namespace BusinessDashboard.Infrastructure.Notifications;

public class NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public bool IsSeen { get; init; }
}
