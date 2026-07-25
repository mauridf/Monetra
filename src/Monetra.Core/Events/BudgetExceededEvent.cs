using MediatR;

namespace Monetra.Core.Events;

public record BudgetExceededEvent(
    Guid BudgetId,
    Guid UserId,
    Guid CategoryId,
    string CategoryName,
    decimal LimitAmount,
    decimal SpentAmount,
    decimal ExceededPercentage,
    DateTime ExceededAt
) : INotification;
