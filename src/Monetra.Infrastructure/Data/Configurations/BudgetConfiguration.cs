using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.Period)
            .HasColumnName("period")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(b => b.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(b => b.TotalLimit)
            .HasColumnName("total_limit")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(b => b.TotalSpent)
            .HasColumnName("total_spent")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("draft");

        builder.Property(b => b.IsTemplate)
            .HasColumnName("is_template")
            .HasDefaultValue(false);

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Categories)
            .WithOne(c => c.Budget)
            .HasForeignKey(c => c.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
