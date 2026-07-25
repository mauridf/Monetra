using Monetra.Core.Exceptions;

namespace Monetra.Core.ValueObjects;

public class AccountNumber : ValueObject
{
    public string Number { get; }
    public string? Digit { get; }

    private AccountNumber(string number, string? digit)
    {
        Number = number;
        Digit = digit;
    }

    public static AccountNumber Create(string number, string? digit = null)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Número da conta não pode ser vazio.");

        var cleanNumber = number.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");
        var cleanDigit = digit?.Trim().Replace("-", "").Replace(".", "").Replace(" ", "");

        return new AccountNumber(cleanNumber, cleanDigit);
    }

    public string GetFormatted()
    {
        return Digit is not null ? $"{Number}-{Digit}" : Number;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
        yield return Digit ?? string.Empty;
    }

    public override string ToString() => GetFormatted();
}
