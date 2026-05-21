using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Interfaces;

public interface IManagerQueueService
{
    Task<ManagerQueueStatusResponse> GetStatusAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<ManagerQueueStatusResponse> OpenAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<ManagerQueueStatusResponse> CloseAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<ManagerQueueStatusResponse> ResetAsync(
        int userId,
        CancellationToken cancellationToken);
}
