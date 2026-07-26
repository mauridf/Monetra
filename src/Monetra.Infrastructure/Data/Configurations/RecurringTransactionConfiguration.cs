using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
{
    public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
    {
        builder.ToTable("recurring_transactions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.BankAccountId)
            .HasColumnName("bank_account_id")
            .IsRequired();

        builder.Property(r => r.CategoryId)
            .HasColumnName("category_id");

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(r => r.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(r => r.TransactionType)
            .HasColumnName("transaction_type")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(r => r.RecurrenceType)
            .HasColumnName("recurrence_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.IntervalValue)
            .HasColumnName("interval_value")
            .HasDefaultValue(1);

        builder.Property(r => r.IntervalUnit)
            .HasColumnName("interval_unit")
            .HasMaxLength(10);

        builder.Property(r => r.DayOfMonth)
            .HasColumnName("day_of_month");

        builder.Property(r => r.DayOfWeek)
            .HasColumnName("day_of_week");

        builder.Property(r => r.MonthOfYear)
            .HasColumnName("month_of_year");

        builder.Property(r => r.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(r => r.EndDate)
            .HasColumnName("end_date");

        builder.Property(r => r.NextExecution)
            .HasColumnName("next_execution")
            .IsRequired();

        builder.Property(r => r.MaxExecutions)
            .HasColumnName("max_executions");

        builder.Property(r => r.ExecutionsCount)
            .HasColumnName("executions_count")
            .HasDefaultValue(0);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(r => r.AutoCreate)
            .HasColumnName("auto_create")
            .HasDefaultValue(true);

        builder.Property(r => r.NotifyBeforeDays)
            .HasColumnName("notify_before_days");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Relacionamentos
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.BankAccount)
            .WithMany()
            .HasForeignKey(r => r.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.NextExecution, r.IsActive })
            .HasDatabaseName("idx_recurring_next")
            .HasFilter("is_active = true");
    }
}
