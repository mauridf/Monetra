using MediatR;

namespace Monetra.Core;

/// <summary>
/// Aggregate Root - Raiz de agregado no DDD.
/// Gerencia eventos de domínio que serão disparados após persistência.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<INotification> _domainEvents = new();

    /// <summary>
    /// Eventos de domínio pendentes para serem processados.
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Adiciona um evento de domínio para ser processado após o commit.
    /// </summary>
    protected void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove um evento de domínio específico.
    /// </summary>
    protected void RemoveDomainEvent(INotification domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Limpa todos os eventos de domínio (chamado após processamento).
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
