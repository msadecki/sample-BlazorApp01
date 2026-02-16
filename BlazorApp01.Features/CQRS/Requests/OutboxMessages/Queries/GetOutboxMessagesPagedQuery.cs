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
    public async ValueTask<Result<GetOutboxMessagesPagedResponse>> Handle(GetOutboxMessagesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<OutboxMessage>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.EventTypeFilter))
        {
            query = query.Where(outboxMessage => outboxMessage.EventType.Contains(request.EventTypeFilter));
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(outboxMessage => outboxMessage.Status == request.StatusFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "OutboxMessageId"
            : request.SortBy;

        query = ApplySorting(query, sortBy, request.SortAscending);

        var outboxMessages = await query
            .Skip(request.StartIndex)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        return new GetOutboxMessagesPagedResponse(outboxMessages, totalCount);
    }

    private static IQueryable<OutboxMessage> ApplySorting(IQueryable<OutboxMessage> query, string sortBy, bool ascending)
    {
        Expression<Func<OutboxMessage, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "eventtype" => outboxMessage => outboxMessage.EventType,
            "status" => outboxMessage => outboxMessage.Status,
            "createdat" => outboxMessage => outboxMessage.CreatedAt,
            "processedat" => outboxMessage => outboxMessage.ProcessedAt ?? DateTime.MinValue,
            "publishedat" => outboxMessage => outboxMessage.PublishedAt ?? DateTime.MinValue,
            "retrycount" => outboxMessage => outboxMessage.RetryCount,
            "messageid" => outboxMessage => outboxMessage.MessageId,
            _ => outboxMessage => outboxMessage.OutboxMessageId
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}