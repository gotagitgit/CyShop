using System.Text.Json.Serialization;

namespace Orders.Application.IntegrationEvents;

public record IntegrationEvent
{
    [JsonInclude]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonInclude]
    public DateTime CreationDate { get; init; } = DateTime.UtcNow;

    [JsonInclude]
    public string EventTypeName => GetType().FullName!;
}
