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
    public async ValueTask<Result<GetCustomTasksPagedResponse>> Handle(GetCustomTasksPagedQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<CustomTask>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.DescriptionFilter))
        {
            query = query.Where(customTask => customTask.Description.Contains(request.DescriptionFilter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "CustomTaskId"
            : request.SortBy;

        query = ApplySorting(query, sortBy, request.SortAscending);

        var customTasks = await query
            .Skip(request.StartIndex)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        return new GetCustomTasksPagedResponse(customTasks, totalCount);
    }

    private static IQueryable<CustomTask> ApplySorting(IQueryable<CustomTask> query, string sortBy, bool ascending)
    {
        Expression<Func<CustomTask, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "description" => customTask => customTask.Description,
            "status" => customTask => customTask.Status,
            "createdat" => customTask => customTask.CreatedAt,
            "duedate" => customTask => customTask.DueDate,
            "completiondate" => customTask => customTask.CompletionDate ?? DateTime.MinValue,
            "isactive" => customTask => customTask.IsActive,
            "rowversion" => customTask => customTask.RowVersion,
            _ => customTask => customTask.CustomTaskId
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}