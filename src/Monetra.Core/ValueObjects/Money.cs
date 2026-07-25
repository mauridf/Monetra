using Monetra.Core.Exceptions;

namespace Monetra.Core.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string CurrencyCode { get; }

    private Money(decimal amount, string currencyCode)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public static Money Create(decimal amount, string currencyCode = "BRL")
    {
        if (amount < 0)
            throw new DomainException("Valor monetário não pode ser negativo.");

        // Arredondar para 2 casas decimais (precisão financeira)
        var rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        return new Money(rounded, currencyCode.ToUpperInvariant());
    }

    public static Money Zero(string currencyCode = "BRL") => new(0, currencyCode.ToUpperInvariant());

    public Money Add(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new DomainException($"Não é possível somar valores de moedas diferentes: {CurrencyCode} e {other.CurrencyCode}");

        return new Money(Amount + other.Amount, CurrencyCode);
    }

    public Money Subtract(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new DomainException($"Não é possível subtrair valores de moedas diferentes: {CurrencyCode} e {other.CurrencyCode}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainException("Resultado da subtração não pode ser negativo.");

        return new Money(result, CurrencyCode);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return CurrencyCode;
    }

    public override string ToString() => $"{CurrencyCode} {Amount:N2}";
}
