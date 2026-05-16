using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Infrastructure.Repositories;

public sealed class OwnerRegistrationRepository(AppDbContext dbContext)
    : IOwnerRegistrationRepository
{
    public Task<bool> LocationCodeExistsAsync(
        string locationCode,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueLocations.AnyAsync(
            location => location.LocationCode == locationCode,
            cancellationToken);
    }

    public async Task AddRegistrationAsync(
        User owner,
        QueueLocation queueLocation,
        UserLocation userLocation,
        CancellationToken cancellationToken)
    {
        dbContext.Users.Add(owner);
        dbContext.QueueLocations.Add(queueLocation);
        dbContext.UserLocations.Add(userLocation);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
