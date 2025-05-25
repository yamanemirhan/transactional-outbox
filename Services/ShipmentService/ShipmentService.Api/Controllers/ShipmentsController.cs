using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.Events;
using ShipmentService.Api.Data;
using ShipmentService.Api.Models;

namespace ShipmentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController(ShipmentDbContext context) : ControllerBase
{

    [HttpPost("{orderId}/ship")]
    public async Task<IActionResult> ShipOrder(Guid orderId)
    {
        var shipment = await context.Shipments.FindAsync(orderId);
        if (shipment == null)
            return NotFound();

        if (shipment.Status == "Shipped")
            return BadRequest("Shipment already marked as shipped.");

        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            shipment.Status = "Shipped";
            shipment.ShippedAt = DateTime.UtcNow;

            var shippedEvent = new OrderShippedEvent
            {
                OrderId = orderId,
                OccurredOn = DateTime.UtcNow
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = shippedEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(shippedEvent),
                OccurredOn = shippedEvent.OccurredOn,
                ProcessedOn = null,
                Error = null
            };

            context.OutboxMessages.Add(outboxMessage);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { orderId, status = shipment.Status });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetShipment(Guid orderId)
    {
        var shipment = await context.Shipments.FindAsync(orderId);
        if (shipment == null)
            return NotFound();

        return Ok(shipment);
    }
}
