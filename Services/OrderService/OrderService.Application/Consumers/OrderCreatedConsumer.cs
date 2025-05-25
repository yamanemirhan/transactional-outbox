using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.Events;

namespace OrderService.Application.Consumers;
public class OrderCreatedConsumer(
    ILogger<OrderCreatedConsumer> logger, 
    IOrderRepository orderRepository) : IConsumer<OrderCreatedDomainEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedDomainEvent> context)
    {
        var orderId = context.Message.OrderId;

        var exists = await orderRepository.GetByIdAsync(orderId);
        if (exists != null)
        {
            logger.LogInformation("Order {OrderId} zaten işlendi, atlanıyor.", orderId);
            return;
        }

        logger.LogInformation("Order {OrderId} işlendi.", orderId);
    }
}