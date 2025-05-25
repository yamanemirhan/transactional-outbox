namespace OrderService.Application.DTOs;

public record CreateOrderResponse(Guid OrderId, decimal TotalPrice);
