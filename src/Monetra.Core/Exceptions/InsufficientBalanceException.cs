namespace Monetra.Core.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public decimal AvailableBalance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientBalanceException(decimal availableBalance, decimal requestedAmount)
        : base($"Saldo insuficiente. Disponível: {availableBalance:C}, Solicitado: {requestedAmount:C}",
               "INSUFFICIENT_BALANCE")
    {
        AvailableBalance = availableBalance;
        RequestedAmount = requestedAmount;
    }
}
