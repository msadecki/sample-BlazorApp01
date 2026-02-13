using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record CreateCustomTaskCommand(
    string Description,
    CustomTaskStatus Status,
    DateTime CreatedAt,
    DateOnly DueDate,
    DateTime? CompletionDate,
    bool IsActive
) : ICommand<int>;

internal sealed class CreateCustomTaskCommandHandler : ICommandHandler<CreateCustomTaskCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<int>> Handle(CreateCustomTaskCommand command, CancellationToken cancellationToken)
    {
        var customTask = new CustomTask
        {
            Description = command.Description,
            Status = command.Status,
            CreatedAt = command.CreatedAt,
            DueDate = command.DueDate,
            CompletionDate = command.CompletionDate,
            IsActive = command.IsActive
        };

        await _unitOfWork.CustomTasksRepository.AddAsync(customTask, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customTask.CustomTaskId;
    }
}