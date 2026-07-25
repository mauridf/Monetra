using MediatR;

namespace Monetra.Core.Events;

public record AccountBalanceChangedEvent(
    Guid BankAccountId,
    Guid UserId,
    decimal OldBalance,
    decimal NewBalance,
    decimal ChangeAmount,
    Guid? RelatedTransactionId,
    DateTime ChangedAt
) : INotification;
