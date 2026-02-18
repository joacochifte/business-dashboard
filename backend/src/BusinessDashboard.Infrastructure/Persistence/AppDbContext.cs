using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Domain.Products;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Domain.Costs;
using BusinessDashboard.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace BusinessDashboard.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Cost> Costs => Set<Cost>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
