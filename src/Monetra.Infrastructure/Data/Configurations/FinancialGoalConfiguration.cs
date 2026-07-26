using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class FinancialGoalConfiguration : IEntityTypeConfiguration<FinancialGoal>
{
    public void Configure(EntityTypeBuilder<FinancialGoal> builder)
    {
        builder.ToTable("financial_goals");

        builder.HasKey(fg => fg.Id);

        builder.Property(fg => fg.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(fg => fg.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(fg => fg.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(fg => fg.TargetAmount)
            .HasColumnName("target_amount")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(fg => fg.CurrentAmount)
            .HasColumnName("current_amount")
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(fg => fg.TargetDate)
            .HasColumnName("target_date");

        builder.Property(fg => fg.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(false);

        builder.Property(fg => fg.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(fg => fg.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(fg => fg.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(fg => fg.User)
            .WithMany()
            .HasForeignKey(fg => fg.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
