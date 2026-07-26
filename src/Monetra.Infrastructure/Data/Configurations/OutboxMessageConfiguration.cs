using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(om => om.Id);

        builder.Property(om => om.Type)
            .HasColumnName("type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(om => om.Content)
            .HasColumnName("content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(om => om.Headers)
            .HasColumnName("headers")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(om => om.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("pending");

        builder.Property(om => om.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(om => om.MaxRetries)
            .HasColumnName("max_retries")
            .HasDefaultValue(5);

        builder.Property(om => om.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text");

        builder.Property(om => om.ErrorStackTrace)
            .HasColumnName("error_stack_trace")
            .HasColumnType("text");

        builder.Property(om => om.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(om => om.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(om => om.SentAt)
            .HasColumnName("sent_at");

        builder.HasIndex(om => new { om.Status, om.CreatedAt })
            .HasDatabaseName("idx_outbox_pending")
            .HasFilter("status = 'pending'");
    }
}
