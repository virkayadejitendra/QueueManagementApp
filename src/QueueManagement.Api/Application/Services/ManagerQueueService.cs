using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Exceptions;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Services;

public sealed class ManagerQueueService(IManagerQueueRepository repository) : IManagerQueueService
{
    public async Task<ManagerQueueStatusResponse> GetStatusAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        return await CreateStatusResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueStatusResponse> OpenAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        location.IsQueueOpen = true;
        await repository.SaveChangesAsync(cancellationToken);
        return await CreateStatusResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueStatusResponse> CloseAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        location.IsQueueOpen = false;
        await repository.SaveChangesAsync(cancellationToken);
        return await CreateStatusResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueStatusResponse> ResetAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);

        await repository.DeleteEntriesForBusinessDateAsync(
            location.Id,
            businessDate,
            cancellationToken);

        location.IsQueueOpen = false;
        await repository.SaveChangesAsync(cancellationToken);
        return await CreateStatusResponseAsync(location, cancellationToken);
    }

    private async Task<QueueLocation> GetManagedLocationAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return await repository.FindManagedLocationAsync(userId, cancellationToken)
            ?? throw new ManagedQueueNotFoundException("No queue location is assigned to this user.");
    }

    private async Task<ManagerQueueStatusResponse> CreateStatusResponseAsync(
        QueueLocation location,
        CancellationToken cancellationToken)
    {
        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);
        var waitingCount = await repository.CountWaitingEntriesAsync(
            location.Id,
            businessDate,
            cancellationToken);
        var currentTokenNumber = await repository.GetCurrentWaitingTokenAsync(
            location.Id,
            businessDate,
            cancellationToken);

        return new ManagerQueueStatusResponse(
            location.Id,
            location.LocationCode,
            location.BusinessName,
            location.IsQueueOpen,
            waitingCount,
            currentTokenNumber);
    }

    private static DateOnly GetBusinessDate(DateTimeOffset now, TimeOnly queueResetTime)
    {
        var date = DateOnly.FromDateTime(now.LocalDateTime);
        var time = TimeOnly.FromDateTime(now.LocalDateTime);
        return time < queueResetTime ? date.AddDays(-1) : date;
    }
}
