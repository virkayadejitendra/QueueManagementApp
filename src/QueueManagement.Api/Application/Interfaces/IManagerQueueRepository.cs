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

    Task<int?> GetCurrentCalledTokenAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<QueueEntry>> GetEntriesForBusinessDateAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<int> GetLastTokenNumberAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken cancellationToken);

    Task<bool> HasCalledEntryAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<QueueEntry?> FindNextWaitingEntryAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<QueueEntry?> FindEntryAsync(
        int queueLocationId,
        int queueEntryId,
        CancellationToken cancellationToken);

    void AddQueueEntry(QueueEntry queueEntry);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task DeleteEntriesForBusinessDateAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);
}
