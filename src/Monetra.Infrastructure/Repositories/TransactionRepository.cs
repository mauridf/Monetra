using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para Transactions com filtros avançados.
/// </summary>
public class TransactionRepository : GenericRepository<Transaction>
{
    public TransactionRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém transações com filtros e paginação.
    /// </summary>
    public async Task<(List<Transaction> Items, int Total)> GetFilteredAsync(
        Guid userId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        TransactionType? type = null,
        Guid? categoryId = null,
        Guid? accountId = null,
        TransactionStatus? status = null,
        string? search = null,
        int page = 1,
        int perPage = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Include(t => t.BankAccount)
            .Where(t => t.UserId == userId)
            .Where(t => t.DeletedAt == null)
            .AsQueryable();

        // Aplicar filtros
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (type.HasValue)
            query = query.Where(t => t.TransactionType == type.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (accountId.HasValue)
            query = query.Where(t => t.BankAccountId == accountId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Description.ToLower().Contains(search.ToLower()));

        // Contar total antes da paginação
        var total = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// Obtém transações pendentes de um usuário.
    /// </summary>
    public async Task<List<Transaction>> GetPendingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.BankAccount)
            .Where(t => t.UserId == userId && t.Status == TransactionStatus.Pending)
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Calcula totais por período (receitas e despesas).
    /// </summary>
    public async Task<(decimal Income, decimal Expense)> GetMonthlyTotalsAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        var transactions = await _dbSet
            .Where(t => t.UserId == userId)
            .Where(t => t.DeletedAt == null)
            .Where(t => t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .Where(t => t.Status == TransactionStatus.Completed || t.Status == TransactionStatus.Reconciled)
            .ToListAsync(cancellationToken);

        var income = transactions
            .Where(t => t.TransactionType == TransactionType.Income)
            .Sum(t => t.Amount);

        var expense = transactions
            .Where(t => t.TransactionType == TransactionType.Expense)
            .Sum(t => t.Amount);

        return (income, expense);
    }
}
