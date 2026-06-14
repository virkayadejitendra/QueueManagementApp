using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Infrastructure.Repositories;

public sealed class CustomerQueueRepository(AppDbContext dbContext) : ICustomerQueueRepository
{
    public Task<QueueLocation?> FindLocationByCodeAsync(
        string locationCode,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueLocations.SingleOrDefaultAsync(
            location => location.LocationCode == locationCode,
            cancellationToken);
    }

    public Task<QueueEntry?> FindEntryWithLocationAsync(
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueEntries
            .Include(queueEntry => queueEntry.QueueLocation)
            .SingleOrDefaultAsync(
                queueEntry => queueEntry.Id == queueEntryId,
                cancellationToken);
    }

    public async Task<int> GetLastTokenNumberAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        return await dbContext.QueueEntries
            .Where(queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate)
            .MaxAsync(queueEntry => (int?)queueEntry.TokenNumber, cancellationToken)
            ?? 0;
    }

    public Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueEntries.AnyAsync(
            queueEntry => queueEntry.TrackingToken == trackingToken,
            cancellationToken);
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

    public Task<int> CountWaitingEntriesBeforeOrAtAsync(
        int queueLocationId,
        DateOnly businessDate,
        int sortOrder,
        int tokenNumber,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueEntries.CountAsync(
            queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate
                && queueEntry.Status == QueueEntryStatuses.Waiting
                && (queueEntry.SortOrder < sortOrder
                    || (queueEntry.SortOrder == sortOrder && queueEntry.TokenNumber <= tokenNumber)),
            cancellationToken);
    }

    public Task<QueueEntry?> GetCurrentCalledEntryAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        return dbContext.QueueEntries
            .Where(queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate
                && queueEntry.Status == QueueEntryStatuses.Called)
            .OrderBy(queueEntry => queueEntry.SortOrder)
            .ThenBy(queueEntry => queueEntry.TokenNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<QueueEntry?> GetLastServedEntryAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        var servedEntries = await dbContext.QueueEntries
            .Where(queueEntry =>
                queueEntry.QueueLocationId == queueLocationId
                && queueEntry.BusinessDate == businessDate
                && queueEntry.Status == QueueEntryStatuses.Served)
            .ToListAsync(cancellationToken);

        return servedEntries
            .OrderByDescending(queueEntry => queueEntry.ServedAt)
            .ThenByDescending(queueEntry => queueEntry.TokenNumber)
            .FirstOrDefault();
    }

    public async Task AddQueueEntryAsync(
        QueueEntry queueEntry,
        CancellationToken cancellationToken)
    {
        dbContext.QueueEntries.Add(queueEntry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
