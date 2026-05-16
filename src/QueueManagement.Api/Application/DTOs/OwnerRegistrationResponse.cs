namespace QueueManagement.Api.Application.DTOs;

public sealed record OwnerRegistrationResponse(
    int OwnerId,
    int QueueLocationId,
    string LocationCode,
    string BusinessName,
    string? LocationName,
    string Role);
