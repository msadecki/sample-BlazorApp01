using System.ComponentModel.DataAnnotations;
using BlazorApp01.Domain.Abstractions;
using BlazorApp01.Domain.Enums;

namespace BlazorApp01.Domain.Models.EventStore;

/// <summary>
/// Represents an outbox message for reliable event publishing.
/// </summary>
public sealed class OutboxMessage : IEntity
{
  [Key]
  public long OutboxMessageId { get; set; }

  [Required]
  public Guid MessageId { get; set; }

  [Required, StringLength(500)]
  public required string EventType { get; set; }

  [Required]
  public required string EventData { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime? ProcessedAt { get; set; }

  public DateTime? PublishedAt { get; set; }

  public int RetryCount { get; set; }

  [StringLength(2000)]
  public string? Error { get; set; }

  [Required]
  public OutboxMessageStatus Status { get; set; }
}
