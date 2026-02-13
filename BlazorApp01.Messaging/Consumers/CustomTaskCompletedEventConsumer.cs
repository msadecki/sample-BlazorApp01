using BlazorApp01.Domain.Events.CustomTasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.Messaging.Consumers;

/// <summary>
/// Consumer that processes CustomTaskCompletedEvent messages from RabbitMQ.
/// </summary>
public sealed class CustomTaskCompletedEventConsumer(ILogger<CustomTaskCompletedEventConsumer> logger) : IConsumer<CustomTaskCompletedEvent>
{
    public async Task Consume(ConsumeContext<CustomTaskCompletedEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "Processing CustomTaskCompletedEvent: EventId={EventId}, TaskId={CustomTaskId}, CompletionDate={CompletionDate}",
            @event.EventId,
            @event.CustomTaskId,
            @event.CompletionDate);

        try
        {
            // TODO: Implement your business logic here
            // Examples:
            // - Send completion notification
            // - Update statistics/metrics
            // - Archive completed task data
            // - Trigger dependent workflows
            // - Update user dashboards
            // - Send rewards/achievements

            logger.LogInformation(
                "Successfully processed CustomTaskCompletedEvent: EventId={EventId}",
                @event.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process CustomTaskCompletedEvent: EventId={EventId}",
                @event.EventId);
            throw;
        }

        await Task.CompletedTask;
    }
}