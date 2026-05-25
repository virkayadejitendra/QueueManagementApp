namespace QueueManagement.Api.Application.DTOs;

public sealed record ManagerQueueEntryResponse(
    int QueueEntryId,
    int TokenNumber,
    string CustomerName,
    string? Mobile,
    int? PartySize,
    string? ServiceReason,
    string Status,
    int CallCount,
    int SkipCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CalledAt,
    DateTimeOffset? ServedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset? LastSkippedAt);
