using System.Text.Json;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Features.Services.EventStore;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.BackgroundProcessing.Jobs;

/// <summary>
/// Background job that processes outbox messages and publishes them to RabbitMQ.
/// </summary>
internal sealed class OutboxProcessorJob(
    IUnitOfWork unitOfWork,
    IOutboxService outboxService,
    IJsonSerializerOptionsProvider jsonOptionsProvider,
    IPublishEndpoint publishEndpoint,
    ILogger<OutboxProcessorJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Starting OutboxProcessorJob...");

        var pendingMessages = await outboxService.GetPendingMessagesAsync(batchSize: 50);

        logger.LogInformation("Found {Count} pending outbox messages", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    logger.LogWarning(
                        "Could not resolve event type {EventType} for message {MessageId}",
                        message.EventType,
                        message.MessageId);

                    await outboxService.MarkAsFailedAsync(
                        message.OutboxMessageId,
                        $"Could not resolve event type: {message.EventType}");
                    await unitOfWork.SaveChangesAsync();
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.EventData, eventType, jsonOptionsProvider.Options);
                if (domainEvent == null)
                {
                    logger.LogWarning(
                        "Failed to deserialize event data for message {MessageId}",
                        message.MessageId);

                    await outboxService.MarkAsFailedAsync(
                        message.OutboxMessageId,
                        "Failed to deserialize event data");
                    await unitOfWork.SaveChangesAsync();
                    continue;
                }

                // Publish to RabbitMQ via MassTransit
                await publishEndpoint.Publish(domainEvent, eventType);

                await outboxService.MarkAsPublishedAsync(message.OutboxMessageId);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation(
                    "Published event {EventType} with ID {MessageId} to RabbitMQ",
                    message.EventType,
                    message.MessageId);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to publish event {EventType} with ID {MessageId}",
                    message.EventType,
                    message.MessageId);

                await outboxService.MarkAsFailedAsync(message.OutboxMessageId, ex.Message);
                await unitOfWork.SaveChangesAsync();
            }
        }

        logger.LogInformation("Finished OutboxProcessorJob.");
    }
}