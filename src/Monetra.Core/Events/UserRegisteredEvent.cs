using MediatR;

namespace Monetra.Core.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Name,
    string Email,
    DateTime RegisteredAt
) : INotification;
