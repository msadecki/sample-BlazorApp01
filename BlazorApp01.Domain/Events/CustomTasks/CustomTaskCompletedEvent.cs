using BlazorApp01.Domain.Events.Abstractions;

namespace BlazorApp01.Domain.Events.CustomTasks;

/// <summary>
/// Event raised when a custom task is completed.
/// </summary>
public sealed record CustomTaskCompletedEvent : DomainEventBase
{
    public required int CustomTaskId { get; init; }
    public required DateTime CompletionDate { get; init; }
    public required string Description { get; init; }
}