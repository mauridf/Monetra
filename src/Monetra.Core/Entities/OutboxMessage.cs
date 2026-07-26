namespace Monetra.Core.Entities;

public class OutboxMessage : Entity<Guid>
{
    public string Type { get; private set; } = null!;
    public string Content { get; private set; } = null!; // JSON
    public string? Headers { get; private set; } // JSON

    public string Status { get; private set; } = null!; // pending, processing, sent, failed
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? LastError { get; private set; }
    public string? ErrorStackTrace { get; private set; }

    public DateTime? ProcessedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    private OutboxMessage() { }

    private OutboxMessage(string type, string content)
    {
        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        Status = "pending";
        RetryCount = 0;
        MaxRetries = 5;
    }

    /// <summary>
    /// Cria uma nova mensagem de outbox.
    /// </summary>
    public static OutboxMessage Create(string type, string content, string? headers = null)
    {
        return new OutboxMessage(type, content)
        {
            Headers = headers
        };
    }

    /// <summary>
    /// Marca como processando.
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = "processing";
        SetUpdatedAt();
    }

    /// <summary>
    /// Marca como enviada.
    /// </summary>
    public void MarkAsSent()
    {
        Status = "sent";
        SentAt = DateTime.UtcNow;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Registra falha.
    /// </summary>
    public void MarkAsFailed(string error, string? stackTrace = null)
    {
        RetryCount++;
        LastError = error;
        ErrorStackTrace = stackTrace;

        if (RetryCount >= MaxRetries)
        {
            Status = "failed";
            ProcessedAt = DateTime.UtcNow;
        }
        else
        {
            Status = "pending"; // Volta para fila para retry
        }

        SetUpdatedAt();
    }
}
