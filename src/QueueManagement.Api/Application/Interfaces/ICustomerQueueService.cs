using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Interfaces;

public interface ICustomerQueueService
{
    Task<CustomerJoinQueueResponse?> JoinAsync(
        string locationCode,
        CustomerJoinQueueRequest request,
        CancellationToken cancellationToken);
}
