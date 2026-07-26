using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class BankAccountBalanceConfiguration : IEntityTypeConfiguration<BankAccountBalance>
{
    public void Configure(EntityTypeBuilder<BankAccountBalance> builder)
    {
        builder.ToTable("bank_account_balances");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BankAccountId)
            .HasColumnName("bank_account_id")
            .IsRequired();

        builder.Property(b => b.Balance)
            .HasColumnName("balance")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(b => b.BalanceDate)
            .HasColumnName("balance_date")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(b => new { b.BankAccountId, b.BalanceDate })
            .HasDatabaseName("idx_balance_account_unique")
            .IsUnique();

        builder.HasOne(b => b.BankAccount)
            .WithMany(a => a.BalanceHistory)
            .HasForeignKey(b => b.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
