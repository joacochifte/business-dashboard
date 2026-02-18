using BusinessDashboard.Application.Products;
using BusinessDashboard.Application.Sales;
using BusinessDashboard.Infrastructure.Repositories;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Application.Inventory;
using BusinessDashboard.Application.Dashboard;  
using BusinessDashboard.Application.Costs;
using BusinessDashboard.Application.Customers;
using BusinessDashboard.Application.Notifications;
using BusinessDashboard.Infrastructure.Jobs;

namespace BusinessDashboard.Api.Extensions;

public static class ServiceFactory
{
    public static IServiceCollection AddBusinessDashboardServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICostRepository, CostRepository>();
        services.AddScoped<ICostService, CostService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddHostedService<BirthdayNotificationJob>();
        return services;
    }
}
