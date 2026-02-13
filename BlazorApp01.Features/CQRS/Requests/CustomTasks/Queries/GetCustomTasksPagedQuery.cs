using System.Linq.Expressions;
using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Queries;

public sealed record GetCustomTasksPagedQuery(
    string? DescriptionFilter,
    int StartIndex,
    int Count,
    string? SortBy,
    bool SortAscending
) : IQuery<GetCustomTasksPagedResponse>;

public sealed record GetCustomTasksPagedResponse(
    ICollection<CustomTask> Items,
    int TotalCount
);

internal sealed class GetCustomTasksPagedQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetCustomTasksPagedQuery, GetCustomTasksPagedResponse>
{
    public async ValueTask<Result<GetCustomTasksPagedResponse>> Handle(GetCustomTasksPagedQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = unitOfWork.CustomTasksRepository.QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.DescriptionFilter))
        {
            dbQuery = dbQuery.Where(customTask => customTask.Description.Contains(query.DescriptionFilter));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortAscending);
        }

        var items = await dbQuery
            .Skip(query.StartIndex)
            .Take(query.Count)
            .ToListAsync(cancellationToken);

        return new GetCustomTasksPagedResponse(items, totalCount);
    }

    private static IQueryable<CustomTask> ApplySorting(IQueryable<CustomTask> query, string sortBy, bool ascending)
    {
        Expression<Func<CustomTask, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "description" => x => x.Description,
            "status" => x => x.Status,
            "createdat" => x => x.CreatedAt,
            "duedate" => x => x.DueDate,
            "completiondate" => x => x.CompletionDate ?? DateTime.MinValue,
            "isactive" => x => x.IsActive,
            "rowversion" => x => x.RowVersion,
            _ => x => x.CustomTaskId
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}