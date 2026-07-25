using MediatR;

namespace Monetra.Core.Events;

public record InvoiceDueDateNearEvent(
    Guid InvoiceId,
    Guid UserId,
    Guid CreditCardId,
    string CreditCardName,
    decimal TotalAmount,
    DateOnly DueDate,
    int DaysUntilDue,
    DateTime NotifiedAt
) : INotification;
