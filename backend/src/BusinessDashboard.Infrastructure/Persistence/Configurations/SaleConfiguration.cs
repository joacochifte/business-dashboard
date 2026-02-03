using BusinessDashboard.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.Total)
            .HasPrecision(12, 2)
            .IsRequired();

        // Mapea la colección privada "_items"
        builder.Metadata.FindNavigation(nameof(Sale.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<SaleItem>("_items")
            .WithOne()
            .HasForeignKey("sale_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
