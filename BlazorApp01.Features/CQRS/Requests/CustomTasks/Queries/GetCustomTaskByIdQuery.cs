using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Queries;

public sealed record GetCustomTaskByIdQuery(int CustomTaskId) : IQuery<CustomTask?>;

internal sealed class GetCustomTaskByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetCustomTaskByIdQuery, CustomTask?>
{
    public async ValueTask<Result<CustomTask?>> Handle(GetCustomTaskByIdQuery query, CancellationToken cancellationToken)
    {
        return await unitOfWork.CustomTasksRepository
            .QueryAsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomTaskId == query.CustomTaskId, cancellationToken);
    }
}