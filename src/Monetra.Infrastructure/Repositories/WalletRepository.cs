using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para Wallets.
/// </summary>
public class WalletRepository : GenericRepository<Wallet>
{
    public WalletRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Lista carteiras ativas de um usuário.
    /// </summary>
    public async Task<List<Wallet>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.UserId == userId && w.Status == WalletStatus.Active && !w.IsArchived)
            .OrderBy(w => w.DisplayOrder)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém carteira com movimentações.
    /// </summary>
    public async Task<Wallet?> GetWithTransactionsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Transactions.OrderByDescending(t => t.Date).Take(50))
            .Where(w => w.Id == walletId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Calcula progresso de todas as carteiras ativas.
    /// </summary>
    public async Task<List<(Guid WalletId, string Name, decimal Progress)>> GetProgressAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.UserId == userId && w.Status == WalletStatus.Active && !w.IsArchived)
            .Select(w => new { w.Id, w.Name, Progress = w.TargetAmount > 0 ? (w.CurrentAmount / w.TargetAmount) * 100 : 0 })
            .Select(w => ValueTuple.Create(w.Id, w.Name, w.Progress))
            .ToListAsync(cancellationToken);
    }
}
