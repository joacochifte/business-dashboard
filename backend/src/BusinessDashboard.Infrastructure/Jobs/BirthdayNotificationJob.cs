using BusinessDashboard.Domain.Notifications;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessDashboard.Infrastructure.Jobs;

public class BirthdayNotificationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BirthdayNotificationJob> _logger;

    private static readonly int[] TriggerDays = [7, 0];

    public BirthdayNotificationJob(IServiceScopeFactory scopeFactory, ILogger<BirthdayNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running BirthdayNotificationJob.");
            }

            await WaitUntilNextRunAsync(stoppingToken); // next 8AM UTC
        }
    }

    private static async Task WaitUntilNextRunAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1).AddHours(8); // tomorrow 8AM
        if (now.Hour < 8)
            nextRun = now.Date.AddHours(8); // today 8AM if we haven't reached it yet
        var delay = nextRun - now;
        await Task.Delay(delay, ct);
    }

    // Exposed for testing
    public Task RunForTestAsync(CancellationToken ct) => RunAsync(ct);

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        var today = DateTime.UtcNow.Date;
        var customers = await customerRepo.GetAllAsync(ct);

        foreach (var customer in customers.Where(c => c.BirthDate.HasValue))
        {
            foreach (var daysAhead in TriggerDays)
            {
                var triggerDate = today.AddDays(daysAhead);
                var birthDate = customer.BirthDate!.Value;

                if (birthDate.Month != triggerDate.Month || birthDate.Day != triggerDate.Day)
                    continue;

                var title = BuildTitle(customer.Name, daysAhead, triggerDate);

                var alreadyExists = await notificationRepo.ExistsWithTitleAsync(title, ct);
                if (alreadyExists)
                    continue;

                var notification = new Notification(title, today, customer.Id);
                await notificationRepo.AddAsync(notification, ct);

                _logger.LogInformation("Birthday notification created: {Title}", title);
            }
        }
    }

    private static string BuildTitle(string customerName, int daysAhead, DateTime date) => daysAhead switch
    {
        0 => $"🎂 Hoy es el cumpleaños de {customerName}",
        1 => $"🎂 Mañana es el cumpleaños de {customerName}",
        _ => $"🎂 Cumpleaños de {customerName} en {daysAhead} días ({date:dd/MM})"
    };
}
