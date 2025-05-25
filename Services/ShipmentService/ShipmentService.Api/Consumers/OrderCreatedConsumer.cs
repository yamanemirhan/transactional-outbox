using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using ShipmentService.Api.Data;
using ShipmentService.Api.Models;

namespace ShipmentService.Api.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly ShipmentDbContext _context;

    public OrderCreatedConsumer(
        ILogger<OrderCreatedConsumer> logger,
        ShipmentDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderId = context.Message.OrderId;

        var exists = await _context.Shipments.AsNoTracking().FirstOrDefaultAsync(s => s.OrderId == orderId);
        if (exists != null)
        {
            _logger.LogInformation("Shipment for Order {OrderId} already exists. Skipping.", orderId);
            return;
        }

        var shipment = new Shipment
        {
            OrderId = orderId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Shipment created for Order {OrderId} with Pending status.", orderId);
    }
}