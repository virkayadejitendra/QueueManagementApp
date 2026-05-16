using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Interfaces;

public interface IOwnerRegistrationService
{
    Task<OwnerRegistrationResponse> RegisterAsync(
        OwnerRegistrationRequest request,
        CancellationToken cancellationToken);
}
