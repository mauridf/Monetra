using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.FromAccountId)
            .HasColumnName("from_account_id");

        builder.Property(t => t.ToAccountId)
            .HasColumnName("to_account_id");

        builder.Property(t => t.FromTransactionId)
            .HasColumnName("from_transaction_id");

        builder.Property(t => t.ToTransactionId)
            .HasColumnName("to_transaction_id");

        builder.Property(t => t.ToWalletId)
            .HasColumnName("to_wallet_id");

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(t => t.TransferDate)
            .HasColumnName("transfer_date")
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(300);

        builder.Property(t => t.Fee)
            .HasColumnName("fee")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(t => t.FeeAccountId)
            .HasColumnName("fee_account_id");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("completed");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.FromAccount)
            .WithMany()
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToWallet)
            .WithMany()
            .HasForeignKey(t => t.ToWalletId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.FromTransaction)
            .WithMany()
            .HasForeignKey(t => t.FromTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ToTransaction)
            .WithMany()
            .HasForeignKey(t => t.ToTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
