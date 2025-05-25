namespace Shared.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OccurredOn { get; set; }
}

public class OrderShippedEvent
{
    public Guid OrderId { get; set; }
    public DateTime OccurredOn { get; set; }
}
