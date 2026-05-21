using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, int ExpiresIn) CreateToken(
        User user,
        IReadOnlyCollection<string> roles);
}
