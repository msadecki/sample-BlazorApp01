using System.Linq.Expressions;
using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Models.EventStore;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.OutboxMessages.Queries;

public sealed record GetOutboxMessagesPagedQuery(
    string? EventTypeFilter,
    OutboxMessageStatus? StatusFilter,
    int StartIndex,
    int Count,
    string? SortBy,
    bool SortAscending
) : IQuery<GetOutboxMessagesPagedResponse>;

public sealed record GetOutboxMessagesPagedResponse(
    ICollection<OutboxMessage> Items,
    int TotalCount
);

internal sealed class GetOutboxMessagesPagedQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetOutboxMessagesPagedQuery, GetOutboxMessagesPagedResponse>
{
    public async ValueTask<Result<GetOutboxMessagesPagedResponse>> Handle(GetOutboxMessagesPagedQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = unitOfWork.Repository<OutboxMessage>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EventTypeFilter))
        {
            dbQuery = dbQuery.Where(message => message.EventType.Contains(query.EventTypeFilter));
        }

        if (query.StatusFilter.HasValue)
        {
            dbQuery = dbQuery.Where(message => message.Status == query.StatusFilter.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "OutboxMessageId"
            : query.SortBy;

        dbQuery = ApplySorting(dbQuery, sortBy, query.SortAscending);

        var items = await dbQuery
            .Skip(query.StartIndex)
            .Take(query.Count)
            .ToListAsync(cancellationToken);

        return new GetOutboxMessagesPagedResponse(items, totalCount);
    }

    private static IQueryable<OutboxMessage> ApplySorting(IQueryable<OutboxMessage> query, string sortBy, bool ascending)
    {
        Expression<Func<OutboxMessage, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "eventtype" => x => x.EventType,
            "status" => x => x.Status,
            "createdat" => x => x.CreatedAt,
            "processedat" => x => x.ProcessedAt ?? DateTime.MinValue,
            "publishedat" => x => x.PublishedAt ?? DateTime.MinValue,
            "retrycount" => x => x.RetryCount,
            "messageid" => x => x.MessageId,
            _ => x => x.OutboxMessageId
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}