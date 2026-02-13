using Ardalis.Result;
using BlazorApp01.Domain.Enums;
using BlazorApp01.Features.CQRS.MediatorFacade;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;

namespace BlazorApp01.Features.CQRS.Requests.CustomTasks.Commands;

public sealed record AddRandomCustomTaskCommand : ICommand<int>;

internal sealed class AddRandomCustomTaskCommandHandler(ISenderFacade senderFacade) : ICommandHandler<AddRandomCustomTaskCommand, int>
{
    private static readonly string[] TaskDescriptions =
    [
        "Review code changes and provide feedback",
        "Update project documentation",
        "Investigate and fix reported bugs",
        "Optimize database queries",
        "Write unit tests for new features",
        "Refactor legacy code",
        "Conduct security audit",
        "Update dependencies to latest versions",
        "Improve application performance",
        "Prepare demo for stakeholders"
    ];

    public async ValueTask<Result<int>> Handle(AddRandomCustomTaskCommand command, CancellationToken cancellationToken)
    {
        var random = Random.Shared;
        var now = DateTime.UtcNow;
        var statuses = Enum.GetValues<CustomTaskStatus>();
        var randomStatus = statuses[random.Next(statuses.Length)];

        var createCommand = new CreateCustomTaskCommand(
            Description: TaskDescriptions[random.Next(TaskDescriptions.Length)],
            Status: randomStatus,
            CreatedAt: now,
            DueDate: DateOnly.FromDateTime(now.AddDays(random.Next(1, 30))),
            CompletionDate: randomStatus == CustomTaskStatus.Completed ? now : null,
            IsActive: true
        );

        return await senderFacade.SendAsync(createCommand, cancellationToken);
    }
}
