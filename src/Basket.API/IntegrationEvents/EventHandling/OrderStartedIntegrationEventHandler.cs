using Basket.API.IntegrationEvents.Events;
using Basket.API.Repositories;
using EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Basket.API.IntegrationEvents.EventHandling;

public class OrderStartedIntegrationEventHandler(
    IBasketRepository repository,
    ILogger<OrderStartedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        logger.LogInformation(
            "Handling OrderStartedIntegrationEvent: deleting basket for customer {CustomerId} with {OrderId}",
            @event.CustomerId, @event.OrderId);

        await repository.DeleteBasketAsync(@event.CustomerId.ToString());
    }
}
