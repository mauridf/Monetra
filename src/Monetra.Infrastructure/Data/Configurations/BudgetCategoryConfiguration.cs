using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
{
    public void Configure(EntityTypeBuilder<BudgetCategory> builder)
    {
        builder.ToTable("budget_categories");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.BudgetId)
            .HasColumnName("budget_id")
            .IsRequired();

        builder.Property(bc => bc.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(bc => bc.LimitAmount)
            .HasColumnName("limit_amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(bc => bc.SpentAmount)
            .HasColumnName("spent_amount")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(bc => bc.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(bc => bc.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(bc => new { bc.BudgetId, bc.CategoryId })
            .HasDatabaseName("idx_budget_categories_unique")
            .IsUnique();

        builder.HasOne(bc => bc.Budget)
            .WithMany(b => b.Categories)
            .HasForeignKey(bc => bc.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bc => bc.Category)
            .WithMany()
            .HasForeignKey(bc => bc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
