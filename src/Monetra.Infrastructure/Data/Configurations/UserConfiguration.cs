using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monetra.Core.Entities;
using Monetra.Core.ValueObjects;

namespace Monetra.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Propriedades básicas
        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(300)
            .IsRequired();

        // Autenticação
        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.LastPasswordChangeAt)
            .HasColumnName("last_password_change_at");

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0);

        builder.Property(u => u.LockedUntil)
            .HasColumnName("locked_until");

        builder.Property(u => u.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled")
            .HasDefaultValue(false);

        builder.Property(u => u.TwoFactorSecret)
            .HasColumnName("two_factor_secret")
            .HasMaxLength(100);

        // Role
        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(Core.Enums.UserRole.User);

        // Status
        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.IsPremium)
            .HasColumnName("is_premium")
            .HasDefaultValue(false);

        builder.Property(u => u.PremiumUntil)
            .HasColumnName("premium_until");

        // Preferências
        builder.Property(u => u.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("BRL");

        builder.Property(u => u.FiscalYearStart)
            .HasColumnName("fiscal_year_start")
            .HasDefaultValue(1);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        // Índices
        builder.HasIndex(u => u.Email)
            .HasDatabaseName("idx_users_email")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // Relacionamentos
        builder.HasOne(u => u.Person)
            .WithOne(p => p.User)
            .HasForeignKey<Person>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.BankAccounts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Wallets)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.CreditCards)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Categories)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
