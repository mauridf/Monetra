using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Repositório especializado para Notifications.
/// </summary>
public class NotificationRepository : GenericRepository<Notification>
{
    public NotificationRepository(MonetraDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém notificações não lidas de um usuário.
    /// </summary>
    public async Task<List<Notification>> GetUnreadByUserAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.SentAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Conta notificações não lidas.
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Marca todas notificações de um usuário como lidas.
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }

        _dbSet.UpdateRange(unread);
    }
}
