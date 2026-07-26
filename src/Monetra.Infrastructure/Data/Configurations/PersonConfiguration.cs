using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;

namespace Monetra.Infrastructure.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(p => p.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(p => p.Occupation)
            .HasColumnName("occupation")
            .HasMaxLength(100);

        builder.Property(p => p.MonthlyIncomeRange)
            .HasColumnName("monthly_income_range")
            .HasMaxLength(30);

        builder.Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(p => p.State)
            .HasColumnName("state")
            .HasMaxLength(2);

        builder.Property(p => p.Country)
            .HasColumnName("country")
            .HasMaxLength(100)
            .HasDefaultValue("Brasil");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("idx_persons_user")
            .IsUnique();
    }
}
