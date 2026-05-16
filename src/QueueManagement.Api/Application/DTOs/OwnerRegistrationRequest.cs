namespace QueueManagement.Api.Application.DTOs;

public sealed class OwnerRegistrationRequest
{
    public string OwnerName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Mobile { get; init; }
    public string Password { get; init; } = string.Empty;
    public string BusinessName { get; init; } = string.Empty;
    public string? LocationName { get; init; }
    public string Address { get; init; } = string.Empty;
    public string BusinessMobile { get; init; } = string.Empty;
}
