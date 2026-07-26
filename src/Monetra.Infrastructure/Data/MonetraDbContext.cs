using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data.Configurations;
using Monetra.Infrastructure.Data.Interceptors;

namespace Monetra.Infrastructure.Data;

/// <summary>
/// Contexto principal do Entity Framework Core para o Monetra.
/// Centraliza todas as configurações de mapeamento objeto-relacional.
/// </summary>
public class MonetraDbContext : DbContext
{
    // DbSets para cada entidade
    public DbSet<User> Users => Set<User>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankAccountBalance> BankAccountBalances => Set<BankAccountBalance>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceTransaction> InvoiceTransactions => Set<InvoiceTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<FinancialGoal> FinancialGoals => Set<FinancialGoal>();

    private readonly AuditInterceptor _auditInterceptor;
    private readonly DomainEventInterceptor _domainEventInterceptor;

    public MonetraDbContext(
        DbContextOptions<MonetraDbContext> options,
        AuditInterceptor auditInterceptor,
        DomainEventInterceptor domainEventInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
        _domainEventInterceptor = domainEventInterceptor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar todas as configurações desta assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MonetraDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Adicionar interceptors
        optionsBuilder.AddInterceptors(_auditInterceptor, _domainEventInterceptor);

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Sobrescreve SaveChangesAsync para garantir UpdatedAt em todas as entidades.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Disparar eventos de domínio após persistência
        await _domainEventInterceptor.DispatchDomainEventsAsync(this);

        return result;
    }

    /// <summary>
    /// Atualiza campos de auditoria (CreatedAt/UpdatedAt) automaticamente.
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e is { Entity: Core.Entity<Guid>, State: EntityState.Added or EntityState.Modified });

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }

            entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }
    }
}
