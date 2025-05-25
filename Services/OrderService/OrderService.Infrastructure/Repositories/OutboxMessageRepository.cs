using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Application.Messages;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class OutboxMessageRepository(OrderDbContext context) : IOutboxMessageRepository
{

    public async Task AddAsync(OutboxMessage message)
    {
        var entity = new OutboxMessage
        {
            Id = message.Id,
            Type = message.Type,
            Content = message.Content,
            OccurredOn = message.OccurredOn,
            ProcessedOn = message.ProcessedOn,
            Error = message.Error
        };

        await context.OutboxMessages.AddAsync(entity);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int maxCount)
    {
        return await context.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task UpdateAsync(OutboxMessage message)
    {
        context.OutboxMessages.Update((OutboxMessage)message);
        await context.SaveChangesAsync();
    }
}
