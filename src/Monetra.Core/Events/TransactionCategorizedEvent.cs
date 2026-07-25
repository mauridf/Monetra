using MediatR;

namespace Monetra.Core.Events;

public record TransactionCategorizedEvent(
    Guid TransactionId,
    Guid CategoryId,
    DateTime CategorizedAt
) : INotification;
