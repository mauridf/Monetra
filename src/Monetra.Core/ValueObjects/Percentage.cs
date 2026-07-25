using Monetra.Core.Exceptions;

namespace Monetra.Core.ValueObjects;

public class Percentage : ValueObject
{
    public decimal Value { get; } // 0 a 100

    private Percentage(decimal value)
    {
        Value = value;
    }

    public static Percentage Create(decimal value)
    {
        if (value < 0 || value > 100)
            throw new DomainException("Percentual deve estar entre 0 e 100.");

        return new Percentage(Math.Round(value, 2));
    }

    public decimal ToFraction() => Value / 100;

    public static Percentage FromFraction(decimal fraction)
    {
        return Create(Math.Round(fraction * 100, 2));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:F2}%";
}
