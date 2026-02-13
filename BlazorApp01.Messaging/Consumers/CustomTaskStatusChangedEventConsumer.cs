using BlazorApp01.Domain.Events.CustomTasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.Messaging.Consumers;

/// <summary>
/// Consumer that processes CustomTaskStatusChangedEvent messages from RabbitMQ.
/// </summary>
public sealed class CustomTaskStatusChangedEventConsumer(ILogger<CustomTaskStatusChangedEventConsumer> logger) : IConsumer<CustomTaskStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<CustomTaskStatusChangedEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "Processing CustomTaskStatusChangedEvent: EventId={EventId}, TaskId={CustomTaskId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
            @event.EventId,
            @event.CustomTaskId,
            @event.OldStatus,
            @event.NewStatus);

        try
        {
            // TODO: Implement your business logic here
            // Examples:
            // - Update status change history
            // - Send status change notifications
            // - Trigger status-specific workflows
            // - Update reporting dashboards
            // - Log audit trail

            logger.LogInformation(
                "Successfully processed CustomTaskStatusChangedEvent: EventId={EventId}",
                @event.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process CustomTaskStatusChangedEvent: EventId={EventId}",
                @event.EventId);
            throw;
        }

        await Task.CompletedTask;
    }
}