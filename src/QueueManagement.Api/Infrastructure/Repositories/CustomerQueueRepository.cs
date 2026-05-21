using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Application.Interfaces;
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

    public async Task AddQueueEntryAsync(
        QueueEntry queueEntry,
        CancellationToken cancellationToken)
    {
        dbContext.QueueEntries.Add(queueEntry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
