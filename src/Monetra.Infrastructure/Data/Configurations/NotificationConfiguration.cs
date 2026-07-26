using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(n => n.Data)
            .HasColumnName("data")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(n => n.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false);

        builder.Property(n => n.ReadAt)
            .HasColumnName("read_at");

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(n => n.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(30);

        builder.Property(n => n.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("idx_notifications_unread")
            .HasFilter("is_read = false");
    }
}
