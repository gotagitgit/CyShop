using System.Text.Json;

namespace Orders.Application.IntegrationEvents;

public class IntegrationEventLogEntry
{
    public Guid EventId { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public EventState State { get; set; } = EventState.NotPublished;
    public DateTime CreationTime { get; set; }
    public int TimesSent { get; set; }

    /// <summary>
    /// Deserializes the Content back into the original IntegrationEvent type.
    /// </summary>
    public IntegrationEvent DeserializeEvent(Type eventType)
    {
        return (IntegrationEvent)JsonSerializer.Deserialize(Content, eventType)!;
    }
}
