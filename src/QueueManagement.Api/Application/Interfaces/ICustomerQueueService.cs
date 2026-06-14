using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Interfaces;

public interface ICustomerQueueService
{
    Task<CustomerJoinQueueResponse?> JoinAsync(
        string locationCode,
        CustomerJoinQueueRequest request,
        CancellationToken cancellationToken);

    Task<CustomerQueueStatusResponse?> GetStatusAsync(
        int queueEntryId,
        string trackingToken,
        CancellationToken cancellationToken);

    Task<QueueDisplayResponse?> GetDisplayAsync(
        string locationCode,
        CancellationToken cancellationToken);
}
