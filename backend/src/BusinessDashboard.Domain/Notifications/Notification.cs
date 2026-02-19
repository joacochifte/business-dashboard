using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Notifications;

public class Notification : Entity
{
    public string Title { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public bool IsSeen { get; private set; } = false;
    public Guid? CustomerId { get; private set; }

    private Notification() { }

    public Notification(string title, DateTime date, Guid? customerId = null)
    {
        SetTitle(title);
        Date = date;
        CustomerId = customerId;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title is required.");

        Title = title.Trim();
    }

    public void SetDate(DateTime date)
    {
        Date = date;
    }

    public void MarkAsSeen() => IsSeen = true;
    public void MarkAsUnseen() => IsSeen = false;
}
