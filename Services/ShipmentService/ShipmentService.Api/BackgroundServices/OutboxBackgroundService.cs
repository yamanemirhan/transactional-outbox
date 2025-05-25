using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShipmentService.Api.Data;

namespace ShipmentService.Api.BackgroundServices;
public class OutboxBackgroundService(
    ILogger<OutboxBackgroundService> logger,
    IServiceProvider serviceProvider
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<ShipmentDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var messages = await context.OutboxMessages
                    .Where(m => m.ProcessedOn == null)
                    .OrderBy(m => m.OccurredOn)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.Type);
                        if (eventType == null)
                        {
                            throw new Exception($"Event type '{message.Type}' bulunamadı.");
                        }

                        var domainEvent = JsonConvert.DeserializeObject(message.Content, eventType);

                        await publishEndpoint.Publish(domainEvent!, stoppingToken);

                        message.ProcessedOn = DateTime.UtcNow;
                        message.Error = null;

                        await context.SaveChangesAsync(stoppingToken);

                        logger.LogInformation("OutboxMessage {Id} published successfully.", message.Id);
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                        logger.LogError(ex, "Error publishing OutboxMessage {Id}", message.Id);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OutboxBackgroundService loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        logger.LogInformation("OutboxBackgroundService stopped.");
    }
}
