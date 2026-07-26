using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Monetra.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor para logging de operações no banco de dados.
/// Registra queries lentas, erros e alterações de dados.
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AuditInterceptor> _logger;

    public AuditInterceptor(ILogger<AuditInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Registra em log as alterações que estão sendo persistidas.
    /// </summary>
    private void LogChanges(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0) return;

        _logger.LogInformation("Persistindo {Count} alterações no banco de dados", entries.Count);

        foreach (var entry in entries.Take(10)) // Limitar log a 10 entradas
        {
            _logger.LogDebug(
                "Entidade: {Entity} | Estado: {State} | ID: {Id}",
                entry.Entity.GetType().Name,
                entry.State,
                entry.Property("Id").CurrentValue);
        }

        if (entries.Count > 10)
        {
            _logger.LogDebug("... e mais {Count} alterações", entries.Count - 10);
        }
    }
}
