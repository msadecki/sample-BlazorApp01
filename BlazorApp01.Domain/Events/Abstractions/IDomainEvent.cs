namespace BlazorApp01.Domain.Events.Abstractions;

/// <summary>
/// Marker interface for domain events that represent something that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Type name of the event for deserialization.
    /// </summary>
    string EventType { get; }
}