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
        // Include UserLocations because login responses and JWT tokens need the user's owner/manager roles.
        return dbContext.Users
            .Include(user => user.UserLocations)
            // Inactive users should not be able to log in, even if their password is correct.
            .Where(user => user.IsActive)
            // The normalized identifier can match either login method: email or mobile number.
            .SingleOrDefaultAsync(
                user => user.Email == identifier || user.Mobile == identifier,
                cancellationToken);
    }
}
