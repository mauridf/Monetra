using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.BankAccountId)
            .HasColumnName("bank_account_id")
            .IsRequired();

        builder.Property(t => t.CategoryId)
            .HasColumnName("category_id");

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .HasColumnName("transaction_type")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.BalanceBefore)
            .HasColumnName("balance_before")
            .HasColumnType("decimal(15,2)");

        builder.Property(t => t.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("decimal(15,2)");

        builder.Property(t => t.TransactionDate)
            .HasColumnName("transaction_date")
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.PaidDate)
            .HasColumnName("paid_date");

        builder.Property(t => t.CompetenceDate)
            .HasColumnName("competence_date");

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(t => t.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.DocumentNumber)
            .HasColumnName("document_number")
            .HasMaxLength(100);

        builder.Property(t => t.ReceiptUrl)
            .HasColumnName("receipt_url")
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TransactionStatus.Pending);

        builder.Property(t => t.IsRecurring)
            .HasColumnName("is_recurring")
            .HasDefaultValue(false);

        builder.Property(t => t.RecurrenceId)
            .HasColumnName("recurrence_id");

        builder.Property(t => t.IsReconciled)
            .HasColumnName("is_reconciled")
            .HasDefaultValue(false);

        builder.Property(t => t.ReconciledAt)
            .HasColumnName("reconciled_at");

        builder.Property(t => t.Tags)
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .HasDefaultValue(Array.Empty<string>());

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.DeletedAt)
            .HasColumnName("deleted_at");

        // Relacionamentos
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.BankAccount)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.RecurringTransaction)
            .WithMany(r => r.GeneratedTransactions)
            .HasForeignKey(t => t.RecurrenceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(t => new { t.UserId, t.TransactionDate })
            .HasDatabaseName("idx_transactions_user_date")
            .IsDescending(false, true);

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("idx_transactions_user");

        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("idx_transactions_category");

        builder.HasIndex(t => new { t.UserId, t.Status })
            .HasDatabaseName("idx_transactions_status");

        // Filtro global para soft delete
        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}
