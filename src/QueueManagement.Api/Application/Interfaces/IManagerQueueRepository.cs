using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Interfaces;

public interface IManagerQueueRepository
{
    Task<QueueLocation?> FindManagedLocationAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<int> CountWaitingEntriesAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<int?> GetCurrentWaitingTokenAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task DeleteEntriesForBusinessDateAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);
}
