using System.ComponentModel.DataAnnotations;
using BlazorApp01.Domain.Abstractions;

namespace BlazorApp01.Domain.Models.EventStore;

/// <summary>
/// Represents a stored domain event in the event store.
/// </summary>
public sealed class StoredEvent : IEntity
{
    [Key]
    public long StoredEventId { get; set; }

    [Required]
    public Guid EventId { get; set; }

    [Required, StringLength(500)]
    public required string EventType { get; set; }

    [Required, StringLength(200)]
    public required string AggregateType { get; set; }

    [Required, StringLength(200)]
    public required string AggregateId { get; set; }

    public required int Version { get; set; }

    [Required]
    public required string EventData { get; set; }

    [Required]
    public DateTime OccurredAt { get; set; }

    public DateTime StoredAt { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string? CorrelationId { get; set; }

    [StringLength(200)]
    public string? CausationId { get; set; }

    [StringLength(200)]
    public string? UserId { get; set; }
}