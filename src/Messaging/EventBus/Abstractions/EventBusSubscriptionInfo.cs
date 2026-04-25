using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EventBus.Abstractions;

public class EventBusSubscriptionInfo
{
    public Dictionary<string, Type> EventTypes { get; } = [];

    public JsonSerializerOptions JsonSerializerOptions { get; } = new(DefaultSerializerOptions);

    internal static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault
            ? CreateDefaultTypeResolver()
            : JsonTypeInfoResolver.Combine()
    };

#pragma warning disable IL2026
#pragma warning disable IL3050
    private static IJsonTypeInfoResolver CreateDefaultTypeResolver()
        => new DefaultJsonTypeInfoResolver();
#pragma warning restore IL3050
#pragma warning restore IL2026
}
