using MediatR;

namespace Monetra.Core.Events;

public record WalletContributedEvent(
    Guid WalletId,
    Guid UserId,
    decimal Amount,
    decimal NewBalance,
    DateTime ContributedAt
) : INotification;
