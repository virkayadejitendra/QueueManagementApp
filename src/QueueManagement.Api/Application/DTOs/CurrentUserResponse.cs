namespace QueueManagement.Api.Application.DTOs;

public sealed record CurrentUserResponse(
    int UserId,
    string Name,
    IReadOnlyCollection<string> Roles);
