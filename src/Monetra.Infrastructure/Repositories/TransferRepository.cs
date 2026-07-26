using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

public class TransferRepository : GenericRepository<Transfer>
{
    public TransferRepository(MonetraDbContext context) : base(context)
    {
    }

    public async Task<List<Transfer>> GetByUserAsync(Guid userId, int page = 1, int perPage = 20, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransferDate)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transfer?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.FromTransaction)
            .Include(t => t.ToTransaction)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}
