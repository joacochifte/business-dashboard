using BusinessDashboard.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");

        builder.HasKey(im => im.Id);

        builder.Property(im => im.ProductId)
            .IsRequired();

        builder.Property(im => im.Type)
            .IsRequired();

        builder.Property(im => im.Reason)
            .IsRequired();

        builder.Property(im => im.Quantity)
            .IsRequired();

        builder.Property(im => im.CreatedAt)
            .IsRequired();
    }
}
