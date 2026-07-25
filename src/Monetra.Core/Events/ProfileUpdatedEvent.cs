using MediatR;

namespace Monetra.Core.Events;

public record ProfileUpdatedEvent(
    Guid UserId,
    Guid PersonId,
    DateTime UpdatedAt
) : INotification;
