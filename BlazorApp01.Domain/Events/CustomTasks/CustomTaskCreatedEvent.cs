using BlazorApp01.Domain.Enums;
using BlazorApp01.Domain.Events.Abstractions;

namespace BlazorApp01.Domain.Events.CustomTasks;

/// <summary>
/// Event raised when a custom task is created.
/// </summary>
public sealed record CustomTaskCreatedEvent : DomainEventBase
{
    public required int CustomTaskId { get; init; }
    public required string Description { get; init; }
    public required CustomTaskStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateOnly DueDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public required bool IsActive { get; init; }
}