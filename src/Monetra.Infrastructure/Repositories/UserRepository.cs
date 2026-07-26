using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Busca usuário por email (apenas ativos e não deletados).
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Email.Value == email.ToLowerInvariant())
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Busca usuário completo com perfil e contas.
    /// </summary>
    public async Task<User?> GetWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Person)
            .Include(u => u.BankAccounts.Where(a => a.IsActive))
            .Include(u => u.Wallets.Where(w => w.Status == Core.Enums.WalletStatus.Active))
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se email já existe (excluindo usuários deletados).
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Email.Value == email.ToLowerInvariant())
            .Where(u => u.DeletedAt == null)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Lista usuários premium que expiraram.
    /// </summary>
    public async Task<List<User>> GetExpiredPremiumUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IsPremium && u.PremiumUntil < DateTime.UtcNow)
            .Where(u => u.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }
}
