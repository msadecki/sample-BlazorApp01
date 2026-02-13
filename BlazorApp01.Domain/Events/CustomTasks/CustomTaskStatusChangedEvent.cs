using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Events.Abstractions;

namespace BlazorApp01.Domain.Events.CustomTasks;

/// <summary>
/// Event raised when a custom task status is changed.
/// </summary>
public sealed record CustomTaskStatusChangedEvent : DomainEventBase
{
    public required int CustomTaskId { get; init; }
    public required CustomTaskStatus OldStatus { get; init; }
    public required CustomTaskStatus NewStatus { get; init; }
    public DateTime? CompletionDate { get; init; }
}