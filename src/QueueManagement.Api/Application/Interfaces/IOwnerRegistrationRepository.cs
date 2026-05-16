using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Interfaces;

public interface IOwnerRegistrationRepository
{
    Task<bool> LocationCodeExistsAsync(string locationCode, CancellationToken cancellationToken);

    Task AddRegistrationAsync(
        User owner,
        QueueLocation queueLocation,
        UserLocation userLocation,
        CancellationToken cancellationToken);
}
