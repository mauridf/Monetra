namespace Monetra.Core.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message, "NOT_FOUND")
    {
    }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} com ID '{id}' não encontrado(a).", "NOT_FOUND")
    {
    }
}
