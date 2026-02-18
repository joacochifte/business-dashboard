using BusinessDashboard.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessDashboard.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(n => n.Date)
            .IsRequired();

        builder.Property(n => n.IsSeen)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
