using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para TransactionCategories.
/// </summary>
public class TransactionCategoryRepository : GenericRepository<TransactionCategory>
{
    public TransactionCategoryRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém árvore completa de categorias (com filhos).
    /// </summary>
    public async Task<List<TransactionCategory>> GetTreeAsync(
        Guid? userId = null,
        string? transactionType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(c => c.Children)
            .Include(c => c.Parent)
            .Where(c => c.IsActive)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value || c.IsSystem);
        else
            query = query.Where(c => c.IsSystem);

        if (!string.IsNullOrWhiteSpace(transactionType))
            query = query.Where(c => c.TransactionType.ToString() == transactionType);

        // Retornar apenas raízes (nível 0), filhos vêm pelo Include
        return await query
            .Where(c => c.Level == 0)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém categorias do sistema.
    /// </summary>
    public async Task<List<TransactionCategory>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsSystem && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}
