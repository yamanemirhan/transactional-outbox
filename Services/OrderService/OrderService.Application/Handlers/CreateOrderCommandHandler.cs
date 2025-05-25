using MediatR;
using Newtonsoft.Json;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Messages;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Handlers;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IOutboxMessageRepository outboxRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(request.Street, request.City, request.District, request.ZipCode);
        var order = new Order(request.CustomerId, address);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            await orderRepository.AddAsync(order);

            foreach (var domainEvent in order.DomainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    Content = JsonConvert.SerializeObject(domainEvent),
                    OccurredOn = domainEvent.OccurredOn,
                    ProcessedOn = null,
                    Error = null
                };
                await outboxRepository.AddAsync(outboxMessage);
            }

            await unitOfWork.CommitAsync();

            order.ClearDomainEvents();

            return new CreateOrderResponse(order.Id, order.GetTotalPrice());
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}
