using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record DeleteCustomTaskCommand(int CustomTaskId) : ICommand<bool>;

internal sealed class DeleteCustomTaskCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteCustomTaskCommand, bool>
{
    public async ValueTask<Result<bool>> Handle(DeleteCustomTaskCommand request, CancellationToken cancellationToken)
    {
        var customTask = await unitOfWork.CommandRepository<CustomTask>()
            .FindAsync(request.CustomTaskId, cancellationToken);

        if (customTask == null)
        {
            return false;
        }

        unitOfWork.CommandRepository<CustomTask>().Remove(customTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}