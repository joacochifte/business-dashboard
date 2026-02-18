using Microsoft.AspNetCore.Mvc;
using BusinessDashboard.Application.Notifications;
using BusinessDashboard.Infrastructure.Notifications;

[ApiController]
[Route("notifications")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = await notificationService.GetAllNotificationsAsync();
        return Ok(notifications);
    }

    [HttpGet("unseen")]
    public async Task<IActionResult> GetUnseen()
    {
        var notifications = await notificationService.GetUnseenNotificationsAsync();
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotificationById(Guid id)
    {
        var notification = await notificationService.GetNotificationByIdAsync(id);
        return Ok(notification);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationCreationDto request)
    {
        return CreatedAtAction(
            nameof(GetNotificationById),
            new { id = await notificationService.CreateNotificationAsync(request) },
            null
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNotification(Guid id, [FromBody] NotificationUpdateDto request)
    {
        await notificationService.UpdateNotificationAsync(id, request);
        return Ok();
    }

    [HttpPatch("{id}/seen")]
    public async Task<IActionResult> MarkAsSeen(Guid id)
    {
        await notificationService.MarkAsSeenAsync(id);
        return Ok();
    }

    [HttpPatch("seen-all")]
    public async Task<IActionResult> MarkAllAsSeen()
    {
        await notificationService.MarkAllAsSeenAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        await notificationService.DeleteNotificationAsync(id);
        return Ok();
    }
}
