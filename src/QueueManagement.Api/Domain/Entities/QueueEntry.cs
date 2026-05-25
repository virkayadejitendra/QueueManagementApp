namespace QueueManagement.Api.Domain.Entities;

public sealed class QueueEntry
{
    public int Id { get; set; }
    public int QueueLocationId { get; set; }
    public required string CustomerName { get; set; }
    public string? Mobile { get; set; }
    public int? PartySize { get; set; }
    public string? ServiceReason { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int TokenNumber { get; set; }
    public required string TrackingToken { get; set; }
    public required string Status { get; set; }
    public int CallCount { get; set; }
    public int SkipCount { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CalledAt { get; set; }
    public DateTimeOffset? ServedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public DateTimeOffset? LastSkippedAt { get; set; }

    public QueueLocation? QueueLocation { get; set; }
}
