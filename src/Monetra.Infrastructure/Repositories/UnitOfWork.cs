using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Data;

namespace Monetra.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work.
/// Gerencia transações e coordena saves entre múltiplos repositórios.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly MonetraDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(MonetraDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("{Count} alterações persistidas com sucesso", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar alterações no banco de dados");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            _logger.LogWarning("Transação já está ativa");
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogDebug("Transação iniciada");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogWarning("Nenhuma transação ativa para commit");
            return;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogDebug("Transação confirmada com sucesso");
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogWarning("Nenhuma transação ativa para rollback");
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Transação revertida (rollback)");
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
