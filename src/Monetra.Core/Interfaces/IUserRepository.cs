using Monetra.Core.Entities;

namespace Monetra.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<List<User>> GetExpiredPremiumUsersAsync(CancellationToken cancellationToken = default);
}
