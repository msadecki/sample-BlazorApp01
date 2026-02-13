using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record UpdateCustomTaskCommand(
    int CustomTaskId,
    string Description,
    CustomTaskStatus Status,
    DateTime CreatedAt,
    DateOnly DueDate,
    DateTime? CompletionDate,
    bool IsActive,
    byte[] RowVersion
) : ICommand<bool>;

internal sealed class UpdateCustomTaskCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateCustomTaskCommand, bool>
{
    public async ValueTask<Result<bool>> Handle(UpdateCustomTaskCommand command, CancellationToken cancellationToken)
    {
        var customTask = await unitOfWork.Repository<CustomTask>()
            .Query()
            .FirstOrDefaultAsync(x => x.CustomTaskId == command.CustomTaskId, cancellationToken);

        if (customTask == null)
        {
            return false;
        }

        customTask.Description = command.Description;
        customTask.Status = command.Status;
        customTask.CreatedAt = command.CreatedAt;
        customTask.DueDate = command.DueDate;
        customTask.CompletionDate = command.CompletionDate;
        customTask.IsActive = command.IsActive;
        customTask.RowVersion = command.RowVersion;

        unitOfWork.Repository<CustomTask>().Update(customTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}