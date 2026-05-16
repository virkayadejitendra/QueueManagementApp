namespace QueueManagement.Api.Domain.Entities;

public sealed class UserLocation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QueueLocationId { get; set; }
    public required string Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
    public QueueLocation? QueueLocation { get; set; }
}
