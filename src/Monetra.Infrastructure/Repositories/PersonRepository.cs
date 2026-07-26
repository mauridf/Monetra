using Microsoft.EntityFrameworkCore;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

public class PersonRepository : GenericRepository<Person>, IPersonRepository
{
    public PersonRepository(MonetraDbContext context) : base(context)
    {
    }

    public async Task<Person?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
