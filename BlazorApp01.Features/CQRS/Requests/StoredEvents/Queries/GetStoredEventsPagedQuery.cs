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
    public async ValueTask<Result<GetStoredEventsPagedResponse>> Handle(GetStoredEventsPagedQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = unitOfWork.Repository<StoredEvent>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EventTypeFilter))
        {
            dbQuery = dbQuery.Where(evt => evt.EventType.Contains(query.EventTypeFilter));
        }

        if (!string.IsNullOrWhiteSpace(query.AggregateTypeFilter))
        {
            dbQuery = dbQuery.Where(evt => evt.AggregateType.Contains(query.AggregateTypeFilter));
        }

        if (!string.IsNullOrWhiteSpace(query.AggregateIdFilter))
        {
            dbQuery = dbQuery.Where(evt => evt.AggregateId.Contains(query.AggregateIdFilter));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "StoredEventId"
            : query.SortBy;

        dbQuery = ApplySorting(dbQuery, sortBy, query.SortAscending);

        var items = await dbQuery
            .Skip(query.StartIndex)
            .Take(query.Count)
            .ToListAsync(cancellationToken);

        return new GetStoredEventsPagedResponse(items, totalCount);
    }

    private static IQueryable<StoredEvent> ApplySorting(IQueryable<StoredEvent> query, string sortBy, bool ascending)
    {
        Expression<Func<StoredEvent, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "eventtype" => x => x.EventType,
            "aggregatetype" => x => x.AggregateType,
            "aggregateid" => x => x.AggregateId,
            "version" => x => x.Version,
            "occurredat" => x => x.OccurredAt,
            "storedat" => x => x.StoredAt,
            "eventid" => x => x.EventId,
            _ => x => x.StoredEventId
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}