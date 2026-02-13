using System.ComponentModel.DataAnnotations;
using BlazorApp01.Domain.Abstractions;
using BlazorApp01.Domain.Enums;

namespace BlazorApp01.Domain.Models;

public sealed class CustomTask : IEntity
{
    [Key]
    public int CustomTaskId { get; set; }

    [Required, StringLength(200)]
    public required string Description { get; set; }

    public required CustomTaskStatus Status { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateOnly DueDate { get; set; }

    public required DateTime? CompletionDate { get; set; }

    public required bool IsActive { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
