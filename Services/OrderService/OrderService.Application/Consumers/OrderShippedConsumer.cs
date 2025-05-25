using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.ValueObjects;
using Shared.Events;

namespace OrderService.Application.Consumers;
public class OrderShippedConsumer(ILogger<OrderShippedConsumer> logger,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
    ) : IConsumer<OrderShippedEvent>
{
    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var orderId = context.Message.OrderId;
        var order = await orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found when processing shipped event.", orderId);
            return;
        }

        if (order.Status == OrderStatus.Shipped)
        {
            logger.LogInformation("Order {OrderId} already marked as shipped. Skipping.", orderId);
            return;
        }

        order.MarkAsShipped();
        await unitOfWork.CommitAsync();

        logger.LogInformation("Order {OrderId} status updated to shipped.", orderId);
    }
}