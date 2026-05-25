using System.Security.Cryptography;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Exceptions;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Services;

public sealed class ManagerQueueService(IManagerQueueRepository repository) : IManagerQueueService
{
    private const int TrackingTokenGenerationAttempts = 5;

    public async Task<ManagerQueueStatusResponse> GetStatusAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        return await CreateStatusResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> GetTodayAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
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
        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);
        var entries = await repository.GetEntriesForBusinessDateAsync(
            location.Id,
            businessDate,
            cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in entries.Where(IsActive))
        {
            entry.Status = QueueEntryStatuses.Cancelled;
            entry.CancelledAt = now;
        }

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

    public async Task<ManagerQueueTodayResponse> AddWalkInAsync(
        int userId,
        ManagerWalkInRequest request,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);

        if (!location.IsQueueOpen)
        {
            throw new QueueClosedException("This queue is currently closed.");
        }

        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);
        var tokenNumber = await repository.GetLastTokenNumberAsync(
            location.Id,
            businessDate,
            cancellationToken) + 1;

        repository.AddQueueEntry(new QueueEntry
        {
            QueueLocationId = location.Id,
            CustomerName = request.CustomerName.Trim(),
            Mobile = NormalizeOptional(request.Mobile),
            PartySize = request.PartySize,
            ServiceReason = NormalizeOptional(request.ServiceReason),
            BusinessDate = businessDate,
            TokenNumber = tokenNumber,
            TrackingToken = await CreateUniqueTrackingTokenAsync(cancellationToken),
            Status = QueueEntryStatuses.Waiting,
            SortOrder = tokenNumber
        });

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> CallNextAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);

        if (!location.IsQueueOpen)
        {
            throw new QueueClosedException("This queue is currently closed.");
        }

        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);

        if (await repository.HasCalledEntryAsync(location.Id, businessDate, cancellationToken))
        {
            throw new QueueConflictException("Another customer is already called for this queue.");
        }

        var nextEntry = await repository.FindNextWaitingEntryAsync(
            location.Id,
            businessDate,
            cancellationToken);

        if (nextEntry is not null)
        {
            nextEntry.Status = QueueEntryStatuses.Called;
            nextEntry.CallCount++;
            nextEntry.CalledAt = DateTimeOffset.UtcNow;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> MarkServedAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        var (location, entry) = await GetTodayEntryAsync(userId, queueEntryId, cancellationToken);
        RequireStatus(entry, QueueEntryStatuses.Called, "Only a called customer can be marked as served.");

        entry.Status = QueueEntryStatuses.Served;
        entry.ServedAt = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> MarkNoResponseAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        var (location, entry) = await GetTodayEntryAsync(userId, queueEntryId, cancellationToken);
        RequireStatus(entry, QueueEntryStatuses.Called, "Only a called customer can be marked as no response.");

        entry.Status = QueueEntryStatuses.Waiting;
        entry.CallCount++;
        entry.CalledAt = null;

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> MarkSkippedAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        var (location, entry) = await GetTodayEntryAsync(userId, queueEntryId, cancellationToken);

        if (entry.Status is not QueueEntryStatuses.Called and not QueueEntryStatuses.Waiting)
        {
            throw new QueueConflictException("Only a called or waiting customer can be moved to skipped.");
        }

        entry.Status = QueueEntryStatuses.Skipped;
        entry.SkipCount++;
        entry.CalledAt = null;
        entry.LastSkippedAt = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> RestoreAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        var (location, entry) = await GetTodayEntryAsync(userId, queueEntryId, cancellationToken);
        RequireStatus(entry, QueueEntryStatuses.Skipped, "Only a skipped customer can be restored.");

        entry.Status = QueueEntryStatuses.Waiting;

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
    }

    public async Task<ManagerQueueTodayResponse> CancelAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        var (location, entry) = await GetTodayEntryAsync(userId, queueEntryId, cancellationToken);

        if (!IsActive(entry))
        {
            throw new QueueConflictException("Only an active customer can be cancelled.");
        }

        entry.Status = QueueEntryStatuses.Cancelled;
        entry.CancelledAt = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return await CreateTodayResponseAsync(location, cancellationToken);
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
        var currentTokenNumber = await repository.GetCurrentCalledTokenAsync(
            location.Id,
            businessDate,
            cancellationToken);
        currentTokenNumber ??= await repository.GetCurrentWaitingTokenAsync(
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

    private async Task<ManagerQueueTodayResponse> CreateTodayResponseAsync(
        QueueLocation location,
        CancellationToken cancellationToken)
    {
        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);
        var entries = await repository.GetEntriesForBusinessDateAsync(
            location.Id,
            businessDate,
            cancellationToken);

        var currentCalled = entries
            .Where(entry => entry.Status == QueueEntryStatuses.Called)
            .OrderBy(entry => entry.CalledAt)
            .Select(ToResponse)
            .FirstOrDefault();
        var waitingEntries = entries
            .Where(entry => entry.Status == QueueEntryStatuses.Waiting)
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.TokenNumber)
            .Select(ToResponse)
            .ToList();
        var skippedEntries = entries
            .Where(entry => entry.Status == QueueEntryStatuses.Skipped)
            .OrderByDescending(entry => entry.LastSkippedAt)
            .ThenBy(entry => entry.TokenNumber)
            .Select(ToResponse)
            .ToList();
        var recentServedEntries = entries
            .Where(entry => entry.Status == QueueEntryStatuses.Served)
            .OrderByDescending(entry => entry.ServedAt)
            .ThenByDescending(entry => entry.TokenNumber)
            .Take(5)
            .Select(ToResponse)
            .ToList();

        return new ManagerQueueTodayResponse(
            location.Id,
            location.LocationCode,
            location.BusinessName,
            location.IsQueueOpen,
            waitingEntries.Count,
            currentCalled,
            waitingEntries,
            skippedEntries,
            recentServedEntries);
    }

    private async Task<(QueueLocation Location, QueueEntry Entry)> GetTodayEntryAsync(
        int userId,
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        var location = await GetManagedLocationAsync(userId, cancellationToken);
        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);
        var entry = await repository.FindEntryAsync(location.Id, queueEntryId, cancellationToken);

        if (entry is null || entry.BusinessDate != businessDate)
        {
            throw new QueueEntryNotFoundException("Queue entry was not found for today's managed queue.");
        }

        return (location, entry);
    }

    private async Task<string> CreateUniqueTrackingTokenAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < TrackingTokenGenerationAttempts; attempt++)
        {
            var trackingToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            if (!await repository.TrackingTokenExistsAsync(trackingToken, cancellationToken))
            {
                return trackingToken;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique tracking token.");
    }

    private static ManagerQueueEntryResponse ToResponse(QueueEntry entry)
    {
        return new ManagerQueueEntryResponse(
            entry.Id,
            entry.TokenNumber,
            entry.CustomerName,
            entry.Mobile,
            entry.PartySize,
            entry.ServiceReason,
            entry.Status,
            entry.CallCount,
            entry.SkipCount,
            entry.CreatedAt,
            entry.CalledAt,
            entry.ServedAt,
            entry.CancelledAt,
            entry.LastSkippedAt);
    }

    private static void RequireStatus(
        QueueEntry entry,
        string requiredStatus,
        string errorMessage)
    {
        if (entry.Status != requiredStatus)
        {
            throw new QueueConflictException(errorMessage);
        }
    }

    private static bool IsActive(QueueEntry entry)
    {
        return entry.Status is QueueEntryStatuses.Waiting
            or QueueEntryStatuses.Called
            or QueueEntryStatuses.Skipped;
    }

    private static DateOnly GetBusinessDate(DateTimeOffset now, TimeOnly queueResetTime)
    {
        var date = DateOnly.FromDateTime(now.LocalDateTime);
        var time = TimeOnly.FromDateTime(now.LocalDateTime);
        return time < queueResetTime ? date.AddDays(-1) : date;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
