using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");

        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.WalletId)
            .HasColumnName("wallet_id")
            .IsRequired();

        builder.Property(wt => wt.TransactionId)
            .HasColumnName("transaction_id");

        builder.Property(wt => wt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(wt => wt.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(wt => wt.Type)
            .HasColumnName("type")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(wt => wt.Description)
            .HasColumnName("description")
            .HasMaxLength(300);

        builder.Property(wt => wt.BalanceBefore)
            .HasColumnName("balance_before")
            .HasColumnType("decimal(15,2)");

        builder.Property(wt => wt.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("decimal(15,2)");

        builder.Property(wt => wt.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(wt => wt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(wt => wt.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(wt => wt.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wt => wt.Transaction)
            .WithMany()
            .HasForeignKey(wt => wt.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(wt => wt.User)
            .WithMany()
            .HasForeignKey(wt => wt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
