using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Domain.Notifications;
using BusinessDashboard.Infrastructure.Jobs;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Test.Jobs;

[TestClass]
public class BirthdayNotificationJobTests
{
    private Mock<ICustomerRepository> _customerRepo = null!;
    private Mock<INotificationRepository> _notificationRepo = null!;
    private IServiceProvider _serviceProvider = null!;

    [TestInitialize]
    public void SetUp()
    {
        _customerRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        _notificationRepo = new Mock<INotificationRepository>(MockBehavior.Strict);

        var services = new ServiceCollection();
        services.AddSingleton(_customerRepo.Object);
        services.AddSingleton(_notificationRepo.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    private BirthdayNotificationJob CreateJob()
    {
        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        return new BirthdayNotificationJob(scopeFactory, NullLogger<BirthdayNotificationJob>.Instance);
    }

    [TestMethod]
    public async Task RunAsync_CustomerWithBirthdayToday_ShouldCreateNotification()
    {
        var today = DateTime.UtcNow.Date;
        var customer = new Customer("Juan", birthDate: new DateTime(1990, today.Month, today.Day));

        _customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer });

        _notificationRepo.Setup(r => r.ExistsWithTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Notification? captured = null;
        _notificationRepo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();
        var job = CreateJob();

        await job.RunForTestAsync(cts.Token);

        Assert.IsNotNull(captured);
        StringAssert.Contains(captured.Title, "Juan");
        StringAssert.Contains(captured.Title, "Hoy");

        _notificationRepo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task RunAsync_CustomerWithBirthdayIn7Days_ShouldCreateNotification()
    {
        var triggerDate = DateTime.UtcNow.Date.AddDays(7);
        var customer = new Customer("María", birthDate: new DateTime(1985, triggerDate.Month, triggerDate.Day));

        _customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer });

        _notificationRepo.Setup(r => r.ExistsWithTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Notification? captured = null;
        _notificationRepo.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();
        var job = CreateJob();

        await job.RunForTestAsync(cts.Token);

        Assert.IsNotNull(captured);
        StringAssert.Contains(captured.Title, "María");
        StringAssert.Contains(captured.Title, "7 días");
    }

    [TestMethod]
    public async Task RunAsync_NotificationAlreadyExists_ShouldNotCreateDuplicate()
    {
        var today = DateTime.UtcNow.Date;
        var customer = new Customer("Pedro", birthDate: new DateTime(1990, today.Month, today.Day));

        _customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer });

        _notificationRepo.Setup(r => r.ExistsWithTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var cts = new CancellationTokenSource();
        var job = CreateJob();

        await job.RunForTestAsync(cts.Token);

        _notificationRepo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task RunAsync_CustomerWithoutBirthDate_ShouldNotCreateNotification()
    {
        var customer = new Customer("Luis");

        _customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer });

        using var cts = new CancellationTokenSource();
        var job = CreateJob();

        await job.RunForTestAsync(cts.Token);

        _notificationRepo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task RunAsync_CustomerBirthdayNotInTriggerDays_ShouldNotCreateNotification()
    {
        var otherDate = DateTime.UtcNow.Date.AddDays(3);
        var customer = new Customer("Carlos", birthDate: new DateTime(1990, otherDate.Month, otherDate.Day));

        _customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer });

        using var cts = new CancellationTokenSource();
        var job = CreateJob();

        await job.RunForTestAsync(cts.Token);

        _notificationRepo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
