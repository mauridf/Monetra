using MediatR;

namespace Monetra.Core.Events;

public record WalletGoalReachedEvent(
    Guid WalletId,
    Guid UserId,
    string WalletName,
    decimal TargetAmount,
    DateTime ReachedAt
) : INotification;
