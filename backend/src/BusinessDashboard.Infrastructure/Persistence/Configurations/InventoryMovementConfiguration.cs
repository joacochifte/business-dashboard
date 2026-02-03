using BusinessDashboard.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("inventory_movements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProductId)
            .IsRequired();

        builder.Property(m => m.Quantity)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        // Enums como string para legibilidad (portfolio win)
        builder.Property(m => m.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasConversion<string>()
            .IsRequired();
    }
}
