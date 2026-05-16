using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Services;

public sealed class OwnerRegistrationService(
    IOwnerRegistrationRepository repository,
    ILocationCodeGenerator locationCodeGenerator,
    IPasswordHasher<User> passwordHasher,
    ILogger<OwnerRegistrationService> logger) : IOwnerRegistrationService
{
    private const int LocationCodeGenerationAttempts = 5;

    public async Task<OwnerRegistrationResponse> RegisterAsync(
        OwnerRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering owner for business {BusinessName}.", request.BusinessName);

        var owner = new User
        {
            Name = request.OwnerName.Trim(),
            Email = NormalizeOptional(request.Email),
            Mobile = NormalizeOptional(request.Mobile),
            PasswordHash = string.Empty
        };

        owner.PasswordHash = passwordHasher.HashPassword(owner, request.Password);

        var queueLocation = new QueueLocation
        {
            BusinessName = request.BusinessName.Trim(),
            LocationName = NormalizeOptional(request.LocationName),
            Address = request.Address.Trim(),
            Mobile = request.BusinessMobile.Trim(),
            LocationCode = await CreateUniqueLocationCodeAsync(cancellationToken)
        };

        var userLocation = new UserLocation
        {
            User = owner,
            QueueLocation = queueLocation,
            Role = UserLocationRoles.Owner
        };

        await repository.AddRegistrationAsync(owner, queueLocation, userLocation, cancellationToken);

        logger.LogInformation(
            "Registered owner {OwnerId} for queue location {QueueLocationId}.",
            owner.Id,
            queueLocation.Id);

        return new OwnerRegistrationResponse(
            owner.Id,
            queueLocation.Id,
            queueLocation.LocationCode,
            queueLocation.BusinessName,
            queueLocation.LocationName,
            userLocation.Role);
    }

    private async Task<string> CreateUniqueLocationCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < LocationCodeGenerationAttempts; attempt++)
        {
            var locationCode = locationCodeGenerator.Create();

            if (!await repository.LocationCodeExistsAsync(locationCode, cancellationToken))
            {
                return locationCode;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique location code.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
