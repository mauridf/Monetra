using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(al => al.Action)
            .HasColumnName("action")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(al => al.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(50);

        builder.Property(al => al.EntityId)
            .HasColumnName("entity_id");

        builder.Property(al => al.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(al => al.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(al => al.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(al => al.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet");

        builder.Property(al => al.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text");

        builder.Property(al => al.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
