namespace BlazorApp01.Domain.Events.Abstractions;

/// <summary>
/// Base class for domain events providing common properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; }

    protected DomainEventBase()
    {
        EventType = GetType().FullName ?? GetType().Name;
    }
}