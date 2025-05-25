using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Commands;
public record CreateOrderCommand(
    Guid CustomerId,
    string Street,
    string City,
    string District,
    string ZipCode,
    List<CreateOrderItemDto> Items
) : IRequest<CreateOrderResponse>;

public record CreateOrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
