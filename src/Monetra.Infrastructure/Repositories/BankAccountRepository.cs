using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para BankAccounts.
/// </summary>
public class BankAccountRepository : GenericRepository<BankAccount>
{
    public BankAccountRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Lista contas ativas de um usuário com ordenação.
    /// </summary>
    public async Task<List<BankAccount>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.IsActive && !a.IsArchived)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém conta com histórico de saldo.
    /// </summary>
    public async Task<BankAccount?> GetWithBalanceHistoryAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.BalanceHistory.OrderByDescending(h => h.BalanceDate).Take(30))
            .Where(a => a.Id == accountId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Calcula o saldo total de todas as contas ativas de um usuário.
    /// </summary>
    public async Task<decimal> GetTotalBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotals)
            .SumAsync(a => a.Balance, cancellationToken);
    }
}
