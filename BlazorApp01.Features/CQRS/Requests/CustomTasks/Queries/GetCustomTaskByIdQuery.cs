using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Queries;

public sealed record GetCustomTaskByIdQuery(int CustomTaskId) : IQuery<CustomTask?>;

internal sealed class GetCustomTaskByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetCustomTaskByIdQuery, CustomTask?>
{
    public async ValueTask<Result<CustomTask?>> Handle(GetCustomTaskByIdQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<CustomTask>()
            .FindAsNoTrackingAsync(request.CustomTaskId, cancellationToken);
    }
}