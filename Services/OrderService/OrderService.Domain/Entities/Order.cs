using OrderService.Domain.Base;
using OrderService.Domain.Events;
using OrderService.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();
    public Guid CustomerId { get; private set; }
    public Address DeliveryAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    [NotMapped]
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(Guid customerId, Address deliveryAddress)
    {
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Created;

        AddDomainEvent(new OrderCreatedDomainEvent(Id, CustomerId));
    }

    public void AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        _items.Add(new OrderItem(productId, productName, price, quantity));
    }

    public decimal GetTotalPrice() => _items.Sum(i => i.GetTotalPrice());

    public void MarkAsShipped()
    {
        Status = OrderStatus.Shipped;
    }
}
