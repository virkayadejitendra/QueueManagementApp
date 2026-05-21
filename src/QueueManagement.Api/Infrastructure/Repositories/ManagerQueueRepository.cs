using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Infrastructure.Repositories;

public sealed class ManagerQueueRepository(AppDbContext dbContext) : IManagerQueueRepository
{
    public Task<QueueLocation?> FindManagedLocationAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return dbContext.UserLocations
            .Where(userLocation =>
                userLocation.UserId == userId
                && (userLocation.Role == UserLocationRoles.Owner || userLocation.Role == UserLocationRoles.Manager))
            .OrderBy(userLocation => userLocation.QueueLocationId)
            .Select(userLocation => userLocation.QueueLocation)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountWaitingEntriesAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueEntries.CountAsync(
            queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate
                && queueEntry.Status == QueueEntryStatuses.Waiting,
            cancellationToken);
    }

    public Task<int?> GetCurrentWaitingTokenAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueEntries
            .Where(queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate
                && queueEntry.Status == QueueEntryStatuses.Waiting)
            .OrderBy(queueEntry => queueEntry.TokenNumber)
            .Select(queueEntry => (int?)queueEntry.TokenNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEntriesForBusinessDateAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        await dbContext.QueueEntries
            .Where(queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
