using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Interfaces;

public interface IManagerQueueService
{
    Task<ManagerQueueStatusResponse> GetStatusAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> GetTodayAsync(
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

    Task<ManagerQueueTodayResponse> AddWalkInAsync(
        int userId,
        ManagerWalkInRequest request,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> CallNextAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> MarkServedAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> MarkNoResponseAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> MarkSkippedAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> RestoreAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken);

    Task<ManagerQueueTodayResponse> CancelAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken);
}
