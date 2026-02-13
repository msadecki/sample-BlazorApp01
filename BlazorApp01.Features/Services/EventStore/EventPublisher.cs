using System.Text.Json;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Events.Abstractions;
using BlazorApp01.Domain.Models.EventStore;
using Microsoft.Extensions.Logging;

namespace BlazorApp01.Features.Services.EventStore;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent;

    Task PublishAsync<TEvent>(IEnumerable<TEvent> domainEvents, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent;
}

internal sealed class EventPublisher(
    IOutboxService outboxService,
    IJsonSerializerOptionsProvider jsonOptionsProvider,
    ILogger<EventPublisher> logger) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        await PublishAsync([domainEvent], cancellationToken);
    }

    public async Task PublishAsync<TEvent>(IEnumerable<TEvent> domainEvents, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                var eventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), jsonOptionsProvider.Options);

                var outboxMessage = new OutboxMessage
                {
                    MessageId = domainEvent.EventId,
                    EventType = domainEvent.EventType,
                    EventData = eventData,
                    CreatedAt = DateTime.UtcNow,
                    Status = OutboxMessageStatus.Pending
                };

                await outboxService.AddOutboxMessageAsync(outboxMessage, cancellationToken);

                logger.LogInformation(
                    "Added event {EventType} with ID {EventId} to outbox",
                    domainEvent.EventType,
                    domainEvent.EventId);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to add event {EventType} to outbox",
                    domainEvent.EventType);
                throw;
            }
        }
    }
}