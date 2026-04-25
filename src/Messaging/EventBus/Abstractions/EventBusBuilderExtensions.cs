using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Abstractions;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder AddSubscription<TEvent, THandler>(this IEventBusBuilder builder)
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        builder.Services.AddKeyedTransient<IIntegrationEventHandler, THandler>(typeof(TEvent));

        // Register the event type so the background consumer knows how to route it
        builder.Services.ConfigureOptions(new ConfigureEventBusSubscription(typeof(TEvent)));

        return builder;
    }

    private sealed class ConfigureEventBusSubscription(Type eventType)
        : Microsoft.Extensions.Options.IConfigureOptions<EventBusSubscriptionInfo>
    {
        public void Configure(EventBusSubscriptionInfo options)
        {
            options.EventTypes[eventType.Name] = eventType;
        }
    }
}
