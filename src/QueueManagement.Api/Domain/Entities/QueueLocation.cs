namespace QueueManagement.Api.Domain.Entities;

public sealed class QueueLocation
{
    public int Id { get; set; }
    public required string BusinessName { get; set; }
    public string? LocationName { get; set; }
    public required string Address { get; set; }
    public required string Mobile { get; set; }
    public required string LocationCode { get; set; }
    public TimeOnly QueueResetTime { get; set; } = new(4, 0);
    public bool IsQueueOpen { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserLocation> UserLocations { get; set; } = [];
}
