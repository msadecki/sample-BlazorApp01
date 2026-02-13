using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
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

internal sealed class UpdateCustomTaskCommandHandler : ICommandHandler<UpdateCustomTaskCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<bool>> Handle(UpdateCustomTaskCommand command, CancellationToken cancellationToken)
    {
        var customTask = await _unitOfWork.CustomTasksRepository
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

        _unitOfWork.CustomTasksRepository.Update(customTask);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}