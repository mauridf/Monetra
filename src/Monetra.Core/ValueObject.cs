namespace Monetra.Core;

/// <summary>
/// Value Object - Objeto de valor imutável.
/// Comparado por todos os seus componentes, não por identidade.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Retorna os componentes que formam a igualdade do Value Object.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not ValueObject other) return false;
        if (GetType() != other.GetType()) return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
