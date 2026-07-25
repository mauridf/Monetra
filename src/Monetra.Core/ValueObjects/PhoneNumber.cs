using System.Text.RegularExpressions;
using Monetra.Core.Exceptions;

namespace Monetra.Core.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Número de telefone não pode ser vazio.");

        // Remove tudo que não for dígito
        var digitsOnly = PhoneDigitsRegex().Replace(phoneNumber, "");

        if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
            throw new DomainException($"Número de telefone inválido: {phoneNumber}");

        // Formata como (XX) XXXXX-XXXX
        var formatted = digitsOnly.Length switch
        {
            10 => $"({digitsOnly[..2]}) {digitsOnly[2..6]}-{digitsOnly[6..]}",
            11 => $"({digitsOnly[..2]}) {digitsOnly[2..7]}-{digitsOnly[7..]}",
            _ => digitsOnly
        };

        return new PhoneNumber(formatted);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex PhoneDigitsRegex();
}
