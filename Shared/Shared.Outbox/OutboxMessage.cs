namespace Shared.Outbox;

public class OutboxMessage
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Полное имя типа события
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// JSON Playload
    /// </summary>
    public string Payload { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { } // Для EF Core

    public OutboxMessage(string type, string payload)
    {
        Id = Guid.NewGuid();
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void MarkAsPublished()
    {
        PublishedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        RetryCount++;
        Error = error;
    }
}