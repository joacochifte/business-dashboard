using BusinessDashboard.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(300);

        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.BirthDate);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.LastPurchaseDate)
            .IsRequired();

        builder.Property(c => c.TotalPurchases)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.TotalLifetimeValue)
            .HasColumnType("numeric(18,2)")
            .IsRequired()
            .HasDefaultValue(0);
    }
}
