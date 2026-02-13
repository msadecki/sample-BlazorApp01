using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record DeleteCustomTaskCommand(int CustomTaskId) : ICommand<bool>;

internal sealed class DeleteCustomTaskCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteCustomTaskCommand, bool>
{
    public async ValueTask<Result<bool>> Handle(DeleteCustomTaskCommand command, CancellationToken cancellationToken)
    {
        var customTask = await unitOfWork.CustomTasksRepository
            .Query()
            .FirstOrDefaultAsync(x => x.CustomTaskId == command.CustomTaskId, cancellationToken);

        if (customTask == null)
        {
            return false;
        }

        unitOfWork.CustomTasksRepository.Remove(customTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}