using Monetra.Core.Enums;

namespace Monetra.Core.Entities;

public class Notification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string? Data { get; private set; } // JSON

    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime SentAt { get; private set; }

    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private Notification() { }

    private Notification(
        Guid userId,
        NotificationType type,
        string title,
        string message)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        IsRead = false;
        SentAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma nova notificação.
    /// </summary>
    public static Notification Create(
        Guid userId,
        string type,
        string title,
        string message,
        string? data = null,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        if (!Enum.TryParse<NotificationType>(type, true, out var notificationType))
            throw new ArgumentException($"Tipo de notificação inválido: {type}");

        return new Notification(userId, notificationType, title, message)
        {
            Data = data,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };
    }

    /// <summary>
    /// Marca notificação como lida.
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
