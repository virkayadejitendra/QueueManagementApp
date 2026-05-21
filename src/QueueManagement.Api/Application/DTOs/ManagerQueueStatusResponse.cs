namespace QueueManagement.Api.Application.DTOs;

public sealed record ManagerQueueStatusResponse(
    int QueueLocationId,
    string LocationCode,
    string BusinessName,
    bool IsQueueOpen,
    int WaitingCount,
    int? CurrentTokenNumber);
