namespace OrderService.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
