using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(w => w.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(w => w.WalletType)
            .HasColumnName("wallet_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(w => w.Icon)
            .HasColumnName("icon")
            .HasMaxLength(50)
            .HasDefaultValue("savings");

        builder.Property(w => w.Color)
            .HasColumnName("color")
            .HasMaxLength(7)
            .HasDefaultValue("#F59E0B");

        builder.Property(w => w.TargetAmount)
            .HasColumnName("target_amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(w => w.CurrentAmount)
            .HasColumnName("current_amount")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(w => w.TargetDate)
            .HasColumnName("target_date");

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(WalletStatus.Active);

        builder.Property(w => w.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        builder.Property(w => w.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(w => w.AutoContribute)
            .HasColumnName("auto_contribute")
            .HasDefaultValue(false);

        builder.Property(w => w.AutoContributeAmount)
            .HasColumnName("auto_contribute_amount")
            .HasColumnType("decimal(15,2)");

        builder.Property(w => w.AutoContributeFrequency)
            .HasColumnName("auto_contribute_frequency")
            .HasMaxLength(20);

        builder.Property(w => w.AutoContributeDay)
            .HasColumnName("auto_contribute_day");

        builder.Property(w => w.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Relacionamentos
        builder.HasOne(w => w.User)
            .WithMany(u => u.Wallets)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
