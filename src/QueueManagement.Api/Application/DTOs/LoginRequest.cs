namespace QueueManagement.Api.Application.DTOs;

public sealed record LoginRequest(
    string Identifier,
    string Password);
