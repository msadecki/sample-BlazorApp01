namespace BlazorApp01.Domain.Enums;

/// <summary>
/// Represents the status of an outbox message.
/// </summary>
public enum OutboxMessageStatus : byte
{
    Pending = 0,
    Processing = 1,
    Published = 2,
    Failed = 3
}