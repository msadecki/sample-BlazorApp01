using System.Text.Json;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Events.Abstractions;
using BlazorApp01.Domain.Models.EventStore;

namespace BlazorApp01.Features.Services.EventStore;

public interface IEventStoreService
{
    Task<StoredEvent> AppendEventAsync(
        IDomainEvent domainEvent,
        string aggregateType,
        string aggregateId,
        int version,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}

internal sealed class EventStoreService(
    IUnitOfWork unitOfWork,
    IJsonSerializerOptionsProvider jsonOptionsProvider) : IEventStoreService
{
    public async Task<StoredEvent> AppendEventAsync(
        IDomainEvent domainEvent,
        string aggregateType,
        string aggregateId,
        int version,
        CancellationToken cancellationToken = default)
    {
        var eventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), jsonOptionsProvider.Options);

        var storedEvent = new StoredEvent
        {
            EventId = domainEvent.EventId,
            EventType = domainEvent.EventType,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Version = version,
            EventData = eventData,
            OccurredAt = domainEvent.OccurredAt,
            StoredAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<StoredEvent>().AddAsync(storedEvent, cancellationToken);
        return storedEvent;
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Repository<StoredEvent>()
            .FindAsync(
                e => e.AggregateType == aggregateType && e.AggregateId == aggregateId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Repository<StoredEvent>()
            .FindAsync(
                e => e.AggregateType == aggregateType &&
                     e.AggregateId == aggregateId &&
                     e.Version >= fromVersion,
                cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<StoredEvent>();
        var query = repository.QueryAsNoTracking()
            .Where(e => e.EventType == eventType);

        if (from.HasValue)
        {
            query = query.Where(e => e.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.OccurredAt <= to.Value);
        }

        return query.OrderBy(e => e.OccurredAt).ToList();
    }
}