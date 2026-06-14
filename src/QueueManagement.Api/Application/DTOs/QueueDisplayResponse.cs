namespace QueueManagement.Api.Application.DTOs;

public sealed record QueueDisplayResponse(
    string LocationCode,
    string BusinessName,
    bool IsQueueOpen,
    int? CurrentCalledTokenNumber,
    string? CurrentCalledCustomerName,
    int? LastServedTokenNumber,
    string? LastServedCustomerName,
    int WaitingCount);
