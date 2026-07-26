using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class InvoiceTransactionConfiguration : IEntityTypeConfiguration<InvoiceTransaction>
{
    public void Configure(EntityTypeBuilder<InvoiceTransaction> builder)
    {
        builder.ToTable("invoice_transactions");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(it => it.CategoryId)
            .HasColumnName("category_id");

        builder.Property(it => it.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(it => it.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(it => it.PurchaseDate)
            .HasColumnName("purchase_date")
            .IsRequired();

        builder.Property(it => it.Installments)
            .HasColumnName("installments")
            .HasDefaultValue(1);

        builder.Property(it => it.InstallmentNumber)
            .HasColumnName("installment_number")
            .HasDefaultValue(1);

        builder.Property(it => it.InstallmentTotal)
            .HasColumnName("installment_total")
            .HasColumnType("decimal(15,2)");

        builder.Property(it => it.MerchantName)
            .HasColumnName("merchant_name")
            .HasMaxLength(200);

        builder.Property(it => it.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(it => it.Invoice)
            .WithMany(i => i.Transactions)
            .HasForeignKey(it => it.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(it => it.Category)
            .WithMany()
            .HasForeignKey(it => it.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
