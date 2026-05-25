namespace QueueManagement.Api.Application.DTOs;

public sealed record ManagerWalkInRequest(
    string CustomerName,
    string? Mobile,
    int? PartySize,
    string? ServiceReason);
