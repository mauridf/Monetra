using MediatR;

namespace Monetra.Core.Events;

public record TransactionCreatedEvent(
    Guid TransactionId,
    Guid UserId,
    Guid BankAccountId,
    decimal Amount,
    string TransactionType,
    DateTime CreatedAt
) : INotification;
