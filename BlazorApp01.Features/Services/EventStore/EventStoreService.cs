using System.Text.Json;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Events.Abstractions;
using BlazorApp01.Domain.Models.EventStore;
using Microsoft.EntityFrameworkCore;

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
            .QueryAsNoTracking()
            .Where(storedEvent => storedEvent.AggregateType == aggregateType && storedEvent.AggregateId == aggregateId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Repository<StoredEvent>()
            .QueryAsNoTracking()
            .Where(storedEvent => storedEvent.AggregateType == aggregateType && storedEvent.AggregateId == aggregateId && storedEvent.Version >= fromVersion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = unitOfWork.Repository<StoredEvent>()
            .QueryAsNoTracking()
            .Where(storedEvent => storedEvent.EventType == eventType);

        if (from.HasValue)
        {
            query = query.Where(storedEvent => storedEvent.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(storedEvent => storedEvent.OccurredAt <= to.Value);
        }

        return await query.OrderBy(storedEvent => storedEvent.OccurredAt).ToListAsync(cancellationToken);
    }
}