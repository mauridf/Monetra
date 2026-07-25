namespace Monetra.Core.Exceptions;

/// <summary>
/// Exceção base para violações de regras de negócio.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
