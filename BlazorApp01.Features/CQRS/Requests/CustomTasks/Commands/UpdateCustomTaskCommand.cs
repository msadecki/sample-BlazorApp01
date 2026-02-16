using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Events.CustomTasks;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using BlazorApp01.Features.Services.EventStore;
using FluentValidation;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record UpdateCustomTaskCommand(
    int CustomTaskId,
    string Description,
    CustomTaskStatus Status,
    DateOnly DueDate,
    DateTime? CompletionDate,
    bool IsActive,
    byte[] RowVersion
) : ICommand<bool>;

internal sealed class UpdateCustomTaskCommandHandler(
    IUnitOfWork unitOfWork,
    IEventStoreService eventStoreService,
    IEventPublisher eventPublisher) : ICommandHandler<UpdateCustomTaskCommand, bool>
{
    public async ValueTask<Result<bool>> Handle(UpdateCustomTaskCommand request, CancellationToken cancellationToken)
    {
        var customTask = await unitOfWork.Repository<CustomTask>()
            .FindAsync(request.CustomTaskId, cancellationToken);

        if (customTask == null)
        {
            return Result<bool>.NotFound($"CustomTask with ID {request.CustomTaskId} not found.");
        }

        var oldStatus = customTask.Status;

        // Update entity
        customTask.Description = request.Description;
        customTask.Status = request.Status;
        customTask.DueDate = request.DueDate;
        customTask.CompletionDate = request.CompletionDate;
        customTask.IsActive = request.IsActive;
        customTask.RowVersion = request.RowVersion;

        unitOfWork.Repository<CustomTask>().Update(customTask);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Raise CustomTaskStatusChangedEvent if status changed
        if (oldStatus != request.Status)
        {
            var statusChangedEvent = new CustomTaskStatusChangedEvent
            {
                CustomTaskId = customTask.CustomTaskId,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                CompletionDate = request.CompletionDate
            };

            await eventStoreService.AppendEventAsync(
                statusChangedEvent,
                aggregateType: nameof(CustomTask),
                aggregateId: customTask.CustomTaskId.ToString(),
                version: 1,
                cancellationToken);

            await eventPublisher.PublishAsync(statusChangedEvent, cancellationToken);

            // If status changed to Completed, also raise CustomTaskCompletedEvent
            if (request.Status == CustomTaskStatus.Completed && request.CompletionDate.HasValue)
            {
                var completedEvent = new CustomTaskCompletedEvent
                {
                    CustomTaskId = customTask.CustomTaskId,
                    CompletionDate = request.CompletionDate.Value,
                    Description = customTask.Description
                };

                await eventStoreService.AppendEventAsync(
                    completedEvent,
                    aggregateType: nameof(CustomTask),
                    aggregateId: customTask.CustomTaskId.ToString(),
                    version: 1,
                    cancellationToken);

                await eventPublisher.PublishAsync(completedEvent, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

internal sealed class UpdateCustomTaskCommandValidator : AbstractValidator<UpdateCustomTaskCommand>
{
    public UpdateCustomTaskCommandValidator()
    {
        RuleFor(x => x.CustomTaskId)
            .GreaterThan(0)
            .WithMessage("CustomTaskId must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid task status");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required");

        RuleFor(x => x.CompletionDate)
            .Must((command, completionDate) => BeValidCompletionDate(completionDate))
            .WithMessage("Completion date cannot be in the future")
            .When(x => x.CompletionDate.HasValue);

        RuleFor(x => x.Status)
            .Must((command, status) => BeConsistentWithCompletionDate(command))
            .WithMessage("Completed tasks must have a completion date, and non-completed tasks cannot have one");

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("RowVersion is required for optimistic concurrency");
    }

    private static bool BeValidCompletionDate(DateTime? completionDate)
    {
        if (!completionDate.HasValue)
        {
            return true;
        }

        return completionDate.Value <= DateTime.UtcNow;
    }

    private static bool BeConsistentWithCompletionDate(UpdateCustomTaskCommand command)
    {
        if (command.Status == CustomTaskStatus.Completed)
        {
            return command.CompletionDate.HasValue;
        }

        return !command.CompletionDate.HasValue;
    }
}