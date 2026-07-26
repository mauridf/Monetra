using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.AccountType)
            .HasColumnName("account_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.BankName)
            .HasColumnName("bank_name")
            .HasMaxLength(100);

        builder.Property(a => a.BankCode)
            .HasColumnName("bank_code")
            .HasMaxLength(10);

        builder.Property(a => a.Agency)
            .HasColumnName("agency")
            .HasMaxLength(20);

        builder.Property(a => a.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(30);

        builder.Property(a => a.AccountDigit)
            .HasColumnName("account_digit")
            .HasMaxLength(5);

        builder.Property(a => a.Balance)
            .HasColumnName("balance")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(a => a.InitialBalance)
            .HasColumnName("initial_balance")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(a => a.BalanceDate)
            .HasColumnName("balance_date");

        builder.Property(a => a.Color)
            .HasColumnName("color")
            .HasMaxLength(7)
            .HasDefaultValue("#10B981");

        builder.Property(a => a.Icon)
            .HasColumnName("icon")
            .HasMaxLength(50)
            .HasDefaultValue("account_balance");

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(a => a.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        builder.Property(a => a.IncludeInTotals)
            .HasColumnName("include_in_totals")
            .HasDefaultValue(true);

        builder.Property(a => a.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Relacionamentos
        builder.HasMany(a => a.BalanceHistory)
            .WithOne(h => h.BankAccount)
            .HasForeignKey(h => h.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.BankAccount)
            .HasForeignKey(t => t.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
