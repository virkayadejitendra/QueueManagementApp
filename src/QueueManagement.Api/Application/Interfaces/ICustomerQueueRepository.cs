using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Interfaces;

public interface ICustomerQueueRepository
{
    Task<QueueLocation?> FindLocationByCodeAsync(
        string locationCode,
        CancellationToken cancellationToken);

    Task<int> GetLastTokenNumberAsync(
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
