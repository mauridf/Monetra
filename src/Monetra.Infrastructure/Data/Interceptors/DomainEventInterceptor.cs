using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Monetra.Core;

namespace Monetra.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor para capturar e disparar eventos de domínio após SaveChanges.
/// </summary>
public class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventInterceptor> _logger;

    public DomainEventInterceptor(IPublisher publisher, ILogger<DomainEventInterceptor> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Dispara todos os eventos de domínio pendentes dos Aggregate Roots.
    /// </summary>
    public async Task DispatchDomainEventsAsync(DbContext context)
    {
        var aggregateRoots = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        _logger.LogDebug("Disparando {Count} eventos de domínio de {Aggregates} agregados",
            aggregateRoots.Sum(a => a.DomainEvents.Count),
            aggregateRoots.Count);

        foreach (var aggregate in aggregateRoots)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                _logger.LogInformation(
                    "Disparando evento: {EventType} para agregado {AggregateType} ID {AggregateId}",
                    domainEvent.GetType().Name,
                    aggregate.GetType().Name,
                    aggregate.Id);

                await _publisher.Publish(domainEvent);
            }
        }
    }
}
