namespace Monetra.Core.Interfaces;

/// <summary>
/// Interface para Unit of Work.
/// Garante atomicidade em operações que envolvem múltiplos repositórios.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Salva todas as alterações em uma transação.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia uma transação explícita.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma a transação.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Desfaz a transação.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
