using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para CreditCards.
/// </summary>
public class CreditCardRepository : GenericRepository<CreditCard>
{
    public CreditCardRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Lista cartões ativos de um usuário com faturas abertas.
    /// </summary>
    public async Task<List<CreditCard>> GetActiveWithOpenInvoicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Invoices.Where(i => i.Status == "open" || i.Status == "closed"))
            .Where(c => c.UserId == userId && c.IsActive && !c.IsArchived)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}
