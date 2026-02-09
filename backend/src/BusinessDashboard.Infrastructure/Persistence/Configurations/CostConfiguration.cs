namespace BusinessDashboard.Infrastructure.Costs;
using BusinessDashboard.Domain.Costs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CostConfiguration : IEntityTypeConfiguration<Cost>
{
    public void Configure(EntityTypeBuilder<Cost> builder)
    {
        builder.ToTable("Costs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.DateIncurred)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(1000);
    }
}