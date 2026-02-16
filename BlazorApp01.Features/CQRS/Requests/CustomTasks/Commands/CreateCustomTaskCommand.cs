using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Events.CustomTasks;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using BlazorApp01.Features.Services.EventStore;
using FluentValidation;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record CreateCustomTaskCommand(
    string Description,
    CustomTaskStatus Status,
    DateTime CreatedAt,
    DateOnly DueDate,
    DateTime? CompletionDate,
    bool IsActive
) : ICommand<int>;

internal sealed class CreateCustomTaskCommandHandler(
    IUnitOfWork unitOfWork,
    IEventStoreService eventStoreService,
    IEventPublisher eventPublisher) : ICommandHandler<CreateCustomTaskCommand, int>
{
    public async ValueTask<Result<int>> Handle(CreateCustomTaskCommand request, CancellationToken cancellationToken)
    {
        var customTask = new CustomTask
        {
            Description = request.Description,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            DueDate = request.DueDate,
            CompletionDate = request.CompletionDate,
            IsActive = request.IsActive
        };

        await unitOfWork.CommandRepository<CustomTask>().AddAsync(customTask, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Create domain event
        var domainEvent = new CustomTaskCreatedEvent
        {
            CustomTaskId = customTask.CustomTaskId,
            Description = customTask.Description,
            Status = customTask.Status,
            CreatedAt = customTask.CreatedAt,
            DueDate = customTask.DueDate,
            CompletionDate = customTask.CompletionDate,
            IsActive = customTask.IsActive
        };

        // Store event in event store
        await eventStoreService.AppendEventAsync(
            domainEvent,
            aggregateType: nameof(CustomTask),
            aggregateId: customTask.CustomTaskId.ToString(),
            version: 1,
            cancellationToken);

        // Add to outbox for publishing
        await eventPublisher.PublishAsync(domainEvent, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customTask.CustomTaskId;
    }
}

internal sealed class CreateCustomTaskCommandValidator : AbstractValidator<CreateCustomTaskCommand>
{
    public CreateCustomTaskCommandValidator()
    {
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
            .WithMessage("Due date is required")
            .Must(dueDate => dueDate >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Due date cannot be in the past");

        RuleFor(x => x.CompletionDate)
            .Must((command, completionDate) => BeValidCompletionDate(command, completionDate))
            .WithMessage("Completion date must be after creation date and cannot be in the future")
            .When(x => x.CompletionDate.HasValue);

        RuleFor(x => x.Status)
            .Must((command, status) => BeConsistentWithCompletionDate(command))
            .WithMessage("Completed tasks must have a completion date, and non-completed tasks cannot have one");
    }

    private static bool BeValidCompletionDate(CreateCustomTaskCommand command, DateTime? completionDate)
    {
        if (!completionDate.HasValue)
        {
            return true;
        }

        return completionDate.Value >= command.CreatedAt &&
               completionDate.Value <= DateTime.UtcNow;
    }

    private static bool BeConsistentWithCompletionDate(CreateCustomTaskCommand command)
    {
        if (command.Status == CustomTaskStatus.Completed)
        {
            return command.CompletionDate.HasValue;
        }

        return !command.CompletionDate.HasValue;
    }
}