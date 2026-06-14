namespace QueueManagement.Api.Application.DTOs;

public sealed record CustomerQueueStatusResponse(
    int QueueEntryId,
    string LocationCode,
    string BusinessName,
    int TokenNumber,
    string Status,
    int? QueuePosition,
    int WaitingCount,
    int? CurrentCalledTokenNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CalledAt,
    DateTimeOffset? ServedAt,
    DateTimeOffset? CancelledAt);
