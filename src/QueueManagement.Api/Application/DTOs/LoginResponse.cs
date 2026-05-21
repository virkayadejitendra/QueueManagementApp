namespace QueueManagement.Api.Application.DTOs;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    int UserId,
    string Name,
    IReadOnlyCollection<string> Roles);
