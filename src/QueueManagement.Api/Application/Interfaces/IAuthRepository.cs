using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Interfaces;

public interface IAuthRepository
{
    Task<User?> FindActiveUserByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken);
}
