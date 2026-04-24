namespace Orders.Application.IntegrationEvents.Events;

public record OrderStartedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName) : IntegrationEvent;
