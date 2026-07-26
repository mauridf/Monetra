using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.CreditCardId)
            .HasColumnName("credit_card_id")
            .IsRequired();

        builder.Property(i => i.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(i => i.ReferenceMonth)
            .HasColumnName("reference_month")
            .IsRequired();

        builder.Property(i => i.ReferenceYear)
            .HasColumnName("reference_year")
            .IsRequired();

        builder.Property(i => i.ClosingDate)
            .HasColumnName("closing_date")
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(i => i.PaymentDate)
            .HasColumnName("payment_date");

        builder.Property(i => i.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(i => i.MinimumPayment)
            .HasColumnName("minimum_payment")
            .HasColumnType("decimal(15,2)");

        builder.Property(i => i.PaidAmount)
            .HasColumnName("paid_amount")
            .HasColumnType("decimal(15,2)");

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("open");

        builder.Property(i => i.PaymentTransactionId)
            .HasColumnName("payment_transaction_id");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(i => new { i.CreditCardId, i.ReferenceMonth, i.ReferenceYear })
            .HasDatabaseName("idx_invoices_card_period")
            .IsUnique();

        builder.HasOne(i => i.CreditCard)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CreditCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Transactions)
            .WithOne(t => t.Invoice)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
