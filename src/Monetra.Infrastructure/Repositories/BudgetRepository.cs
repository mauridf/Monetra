using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para Budgets.
/// </summary>
public class BudgetRepository : GenericRepository<Budget>
{
    public BudgetRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém orçamento ativo do período atual.
    /// </summary>
    public async Task<Budget?> GetCurrentBudgetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _dbSet
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.UserId == userId)
            .Where(b => b.Status == "active")
            .Where(b => b.StartDate <= today && b.EndDate >= today)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém orçamento com progresso detalhado.
    /// </summary>
    public async Task<Budget?> GetWithProgressAsync(Guid budgetId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.Id == budgetId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
