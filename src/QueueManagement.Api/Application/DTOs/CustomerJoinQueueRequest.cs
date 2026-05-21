namespace QueueManagement.Api.Application.DTOs;

public sealed class CustomerJoinQueueRequest
{
    public string CustomerName { get; init; } = string.Empty;
    public string? Mobile { get; init; }
    public int? PartySize { get; init; }
    public string? ServiceReason { get; init; }
}
