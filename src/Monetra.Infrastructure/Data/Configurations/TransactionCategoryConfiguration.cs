using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Infrastructure.Data.Configurations;

public class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.ToTable("transaction_categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .HasColumnName("user_id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(300);

        builder.Property(c => c.Icon)
            .HasColumnName("icon")
            .HasMaxLength(50)
            .HasDefaultValue("category");

        builder.Property(c => c.Color)
            .HasColumnName("color")
            .HasMaxLength(7)
            .HasDefaultValue("#6B7280");

        builder.Property(c => c.TransactionType)
            .HasColumnName("transaction_type")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.ParentId)
            .HasColumnName("parent_id");

        builder.Property(c => c.Level)
            .HasColumnName("level")
            .HasDefaultValue(0);

        builder.Property(c => c.MonthlyBudgetLimit)
            .HasColumnName("monthly_budget_limit")
            .HasColumnType("decimal(15,2)");

        builder.Property(c => c.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Auto-relacionamento (hierarquia)
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
