namespace Monetra.Core;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Fornece comparação por identidade (ID) em vez de referência.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    protected void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
