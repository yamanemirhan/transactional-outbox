using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderService.Application.Interfaces;
using Shared.Mappings;

namespace OrderService.Application.Messages;

public class OutboxProcessor(
    IOutboxMessageRepository outboxRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork,
    ILogger<OutboxProcessor> logger
    )
{
    public async Task ProcessUnprocessedMessagesAsync(int batchSize = 20)
    {
        var messages = await outboxRepository.GetUnprocessedMessagesAsync(batchSize);

        foreach (var message in messages)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                var typeName = message.Type; // "OrderService.Domain.Events.OrderCreatedDomainEvent, OrderService.Domain, Version=..., Culture=..., PublicKeyToken=..."

                Type eventType = Type.GetType(typeName);

                //if (eventType == null)
                //{
                //    var assemblyNameStr = typeName.Split(',')[1].Trim();
                //    var assembly = Assembly.Load(assemblyNameStr);
                //    var pureTypeName = typeName.Split(',')[0].Trim();
                //    eventType = assembly.GetType(pureTypeName);
                //}

                if (eventType == null)
                {
                    throw new Exception($"Event type '{typeName}' bulunamadı.");
                }

                //var @event = JsonConvert.DeserializeObject(message.Content, eventType);
                var domainEvent = JsonConvert.DeserializeObject(message.Content, eventType);

                var contractEvent = DomainEventToContractMapper.Map(domainEvent);

                await publishEndpoint.Publish(contractEvent);

                message.ProcessedOn = DateTime.UtcNow;
                message.Error = null;

                await outboxRepository.UpdateAsync(message);

                await unitOfWork.CommitAsync();

                logger.LogInformation("Outbox message {Id} başarıyla işlendi.", message.Id);
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                message.Error = ex.Message;
                await outboxRepository.UpdateAsync(message);

                logger.LogError(ex, "Outbox message {Id} işlenirken hata oluştu.", message.Id);
            }
        }
    }
}
