using EventBus.Events;

namespace Basket.API.IntegrationEvents.Events;

public record OrderStartedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName) : IntegrationEvent;
