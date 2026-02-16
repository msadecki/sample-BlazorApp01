using System.Linq.Expressions;
using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models.EventStore;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.StoredEvents.Queries;

public sealed record GetStoredEventsPagedQuery(
    string? EventTypeFilter,
    string? AggregateTypeFilter,
    string? AggregateIdFilter,
    int StartIndex,
    int Count,
    string? SortBy,
    bool SortAscending
) : IQuery<GetStoredEventsPagedResponse>;

public sealed record GetStoredEventsPagedResponse(
    ICollection<StoredEvent> Items,
    int TotalCount
);

internal sealed class GetStoredEventsPagedQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetStoredEventsPagedQuery, GetStoredEventsPagedResponse>
{
    public async ValueTask<Result<GetStoredEventsPagedResponse>> Handle(GetStoredEventsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<StoredEvent>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.EventTypeFilter))
        {
            query = query.Where(storedEvent => storedEvent.EventType.Contains(request.EventTypeFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.AggregateTypeFilter))
        {
            query = query.Where(storedEvent => storedEvent.AggregateType.Contains(request.AggregateTypeFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.AggregateIdFilter))
        {
            query = query.Where(storedEvent => storedEvent.AggregateId.Contains(request.AggregateIdFilter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "StoredEventId"
            : request.SortBy;

        query = ApplySorting(query, sortBy, request.SortAscending);

        var storedEvents = await query
            .Skip(request.StartIndex)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        return new GetStoredEventsPagedResponse(storedEvents, totalCount);
    }

    private static IQueryable<StoredEvent> ApplySorting(IQueryable<StoredEvent> query, string sortBy, bool ascending)
    {
        Expression<Func<StoredEvent, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "eventtype" => storedEvent => storedEvent.EventType,
            "aggregatetype" => storedEvent => storedEvent.AggregateType,
            "aggregateid" => storedEvent => storedEvent.AggregateId,
            "version" => storedEvent => storedEvent.Version,
            "occurredat" => storedEvent => storedEvent.OccurredAt,
            "storedat" => storedEvent => storedEvent.StoredAt,
            "eventid" => storedEvent => storedEvent.EventId,
            _ => storedEvent => storedEvent.StoredEventId
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}