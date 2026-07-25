namespace Monetra.Core.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Acesso não autorizado.")
        : base(message, "UNAUTHORIZED")
    {
    }
}
