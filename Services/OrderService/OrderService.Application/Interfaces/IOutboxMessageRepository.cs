using OrderService.Application.Messages;

namespace OrderService.Application.Interfaces;
public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int maxCount);
    Task UpdateAsync(OutboxMessage message);
}
