using BusinessDashboard.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.Total)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(s => s.CustomerId);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(s => s.CustomerName);

        builder.Property(s => s.PaymentMethod)
            .HasMaxLength(50);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey("SaleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Sale.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
