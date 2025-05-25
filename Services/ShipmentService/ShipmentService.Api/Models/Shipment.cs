namespace ShipmentService.Api.Models;
public class Shipment
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedAt { get; set; }
}
