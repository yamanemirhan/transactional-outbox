using OrderService.Domain.Events;
using Shared.Events;

namespace Shared.Mappings;
public static class DomainEventToContractMapper
{
    public static object Map(object domainEvent)
    {
        if (domainEvent is OrderCreatedDomainEvent oce)
        {
            return new OrderCreatedEvent
            {
                OrderId = oce.OrderId,
                CustomerId = oce.CustomerId,
                OccurredOn = oce.OccurredOn
            };
        }

        throw new Exception($"Domain event tipine karşılık gelen contract event bulunamadı: {domainEvent.GetType().Name}");
    }
}

