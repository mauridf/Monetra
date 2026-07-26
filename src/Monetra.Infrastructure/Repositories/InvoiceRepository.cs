using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para Invoices.
/// </summary>
public class InvoiceRepository : GenericRepository<Invoice>
{
    public InvoiceRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém fatura com transações detalhadas.
    /// </summary>
    public async Task<Invoice?> GetWithTransactionsAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Transactions.OrderByDescending(t => t.PurchaseDate))
            .Include(i => i.CreditCard)
            .Where(i => i.Id == invoiceId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém faturas pendentes de pagamento.
    /// </summary>
    public async Task<List<Invoice>> GetPendingPaymentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.CreditCard)
            .Where(i => i.UserId == userId)
            .Where(i => i.Status == "open" || i.Status == "closed")
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se já existe fatura para o período.
    /// </summary>
    public async Task<bool> ExistsForPeriodAsync(
        Guid creditCardId, int month, int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.CreditCardId == creditCardId)
            .Where(i => i.ReferenceMonth == month && i.ReferenceYear == year)
            .AnyAsync(cancellationToken);
    }
}
