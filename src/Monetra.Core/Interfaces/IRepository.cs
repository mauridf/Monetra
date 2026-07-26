using System.Linq.Expressions;

namespace Monetra.Core.Interfaces;

/// <summary>
/// Interface base para repositórios genéricos.
/// Define operações CRUD básicas e queries.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtém entidade por ID.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as entidades.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades por predicado.
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe entidade que atende ao predicado.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma entidade.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona múltiplas entidades.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Remove uma entidade.
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Remove múltiplas entidades.
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Conta entidades que atendem ao predicado.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
