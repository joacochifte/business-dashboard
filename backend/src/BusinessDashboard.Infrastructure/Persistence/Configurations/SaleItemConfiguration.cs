using BusinessDashboard.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.ProductId)
            .IsRequired();

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.UnitPrice)
            .HasPrecision(12, 2)
            .IsRequired();

        // LineTotal es calculado, no lo guardamos
        builder.Ignore(si => si.LineTotal);
    }
}
