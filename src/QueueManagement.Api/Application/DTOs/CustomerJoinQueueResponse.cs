namespace QueueManagement.Api.Application.DTOs;

public sealed record CustomerJoinQueueResponse(
    int QueueEntryId,
    int QueueLocationId,
    string LocationCode,
    int TokenNumber,
    string Status,
    string TrackingToken,
    string StatusUrl);
