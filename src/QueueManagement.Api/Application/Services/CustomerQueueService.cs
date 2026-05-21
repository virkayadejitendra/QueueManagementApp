using System.Security.Cryptography;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Exceptions;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Services;

public sealed class CustomerQueueService(
    ICustomerQueueRepository repository,
    ILogger<CustomerQueueService> logger) : ICustomerQueueService
{
    private const int TrackingTokenGenerationAttempts = 5;

    public async Task<CustomerJoinQueueResponse?> JoinAsync(
        string locationCode,
        CustomerJoinQueueRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedLocationCode = locationCode.Trim().ToUpperInvariant();
        var location = await repository.FindLocationByCodeAsync(
            normalizedLocationCode,
            cancellationToken);

        if (location is null)
        {
            return null;
        }

        if (!location.IsQueueOpen)
        {
            throw new QueueClosedException("This queue is currently closed.");
        }

        var businessDate = GetBusinessDate(DateTimeOffset.Now, location.QueueResetTime);
        var tokenNumber = await repository.GetLastTokenNumberAsync(
            location.Id,
            businessDate,
            cancellationToken) + 1;

        var queueEntry = new QueueEntry
        {
            QueueLocationId = location.Id,
            CustomerName = request.CustomerName.Trim(),
            Mobile = NormalizeOptional(request.Mobile),
            PartySize = request.PartySize,
            ServiceReason = NormalizeOptional(request.ServiceReason),
            BusinessDate = businessDate,
            TokenNumber = tokenNumber,
            TrackingToken = await CreateUniqueTrackingTokenAsync(cancellationToken),
            Status = QueueEntryStatuses.Waiting
        };

        await repository.AddQueueEntryAsync(queueEntry, cancellationToken);

        logger.LogInformation(
            "Customer queue entry {QueueEntryId} joined location {QueueLocationId}.",
            queueEntry.Id,
            location.Id);

        var statusUrl = $"/status/{queueEntry.Id}/{queueEntry.TrackingToken}";

        return new CustomerJoinQueueResponse(
            queueEntry.Id,
            location.Id,
            location.LocationCode,
            queueEntry.TokenNumber,
            queueEntry.Status,
            queueEntry.TrackingToken,
            statusUrl);
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
