using Monetra.Core.Entities;

namespace Monetra.Core.Interfaces;

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
