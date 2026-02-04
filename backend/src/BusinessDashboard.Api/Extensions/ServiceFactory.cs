using BusinessDashboard.Application.Products;
using BusinessDashboard.Application.Sales;
using BusinessDashboard.Infrastructure.Repositories;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessDashboard.Api.Extensions;

public static class ServiceFactory
{
    public static IServiceCollection AddBusinessDashboardServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISalesService, SalesService>();
        return services;
    }
}
