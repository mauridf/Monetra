namespace Monetra.Core.Entities;

public class ActivityLog : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Action { get; private set; } = null!;
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? OldValues { get; private set; } // JSON
    public string? NewValues { get; private set; } // JSON
    public string? Details { get; private set; }   // JSON
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private ActivityLog() { }

    private ActivityLog(Guid userId, string action)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Action = action;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria um novo log de atividade.
    /// </summary>
    public static ActivityLog Create(
        Guid userId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new ActivityLog(userId, action)
        {
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
}
