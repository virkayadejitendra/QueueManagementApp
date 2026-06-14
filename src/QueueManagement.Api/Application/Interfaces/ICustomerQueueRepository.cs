using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Interfaces;

public interface ICustomerQueueRepository
{
    Task<QueueLocation?> FindLocationByCodeAsync(
        string locationCode,
        CancellationToken cancellationToken);

    Task<QueueEntry?> FindEntryWithLocationAsync(
        int queueEntryId,
        CancellationToken cancellationToken);

    Task<int> GetLastTokenNumberAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<int> CountWaitingEntriesAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<int> CountWaitingEntriesBeforeOrAtAsync(
        int queueLocationId,
        DateOnly businessDate,
        int sortOrder,
        int tokenNumber,
        CancellationToken cancellationToken);

    Task<QueueEntry?> GetCurrentCalledEntryAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<QueueEntry?> GetLastServedEntryAsync(
        int queueLocationId,
        DateOnly businessDate,
        CancellationToken cancellationToken);

    Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken cancellationToken);

    Task AddQueueEntryAsync(
        QueueEntry queueEntry,
        CancellationToken cancellationToken);
}
