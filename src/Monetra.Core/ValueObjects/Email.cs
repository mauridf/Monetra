using System.Text.RegularExpressions;
using Monetra.Core.Exceptions;

namespace Monetra.Core.ValueObjects;

public partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email não pode ser vazio.");

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email))
            throw new DomainException($"Email '{email}' não é válido.");

        return new Email(email);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
