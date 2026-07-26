using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.ToTable("credit_cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Brand)
            .HasColumnName("brand")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.LastDigits)
            .HasColumnName("last_digits")
            .HasMaxLength(4);

        builder.Property(c => c.CreditLimit)
            .HasColumnName("credit_limit")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(c => c.AvailableLimit)
            .HasColumnName("available_limit")
            .HasColumnType("decimal(15,2)");

        builder.Property(c => c.ClosingDay)
            .HasColumnName("closing_day")
            .IsRequired();

        builder.Property(c => c.DueDay)
            .HasColumnName("due_day")
            .IsRequired();

        builder.Property(c => c.Color)
            .HasColumnName("color")
            .HasMaxLength(7)
            .HasDefaultValue("#EF4444");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        builder.Property(c => c.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(c => c.User)
            .WithMany(u => u.CreditCards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.CreditCard)
            .HasForeignKey(i => i.CreditCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
