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
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public QueueLocation? QueueLocation { get; set; }
}
