using BlazorApp01.Domain.Events.CustomTasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.Messaging.Consumers;

/// <summary>
/// Consumer that processes CustomTaskCreatedEvent messages from RabbitMQ.
/// </summary>
public sealed class CustomTaskCreatedEventConsumer(ILogger<CustomTaskCreatedEventConsumer> logger) : IConsumer<CustomTaskCreatedEvent>
{
    public async Task Consume(ConsumeContext<CustomTaskCreatedEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "Processing CustomTaskCreatedEvent: EventId={EventId}, TaskId={CustomTaskId}, Description={Description}, Status={Status}",
            @event.EventId,
            @event.CustomTaskId,
            @event.Description,
            @event.Status);

        try
        {
            // TODO: Implement your business logic here
            // Examples:
            // - Update read models/projections
            // - Send notifications (email, SMS, push)
            // - Trigger workflows
            // - Update materialized views
            // - Send to analytics systems
            // - Cache invalidation

            logger.LogInformation(
                "Successfully processed CustomTaskCreatedEvent: EventId={EventId}",
                @event.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process CustomTaskCreatedEvent: EventId={EventId}",
                @event.EventId);
            throw;
        }

        await Task.CompletedTask;
    }
}