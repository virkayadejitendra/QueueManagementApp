using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Infrastructure.Repositories;

public sealed class AuthRepository(AppDbContext dbContext) : IAuthRepository
{
    public Task<User?> FindActiveUserByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.UserLocations)
            .Where(user => user.IsActive)
            .SingleOrDefaultAsync(
                user => user.Email == identifier || user.Mobile == identifier,
                cancellationToken);
    }
}
