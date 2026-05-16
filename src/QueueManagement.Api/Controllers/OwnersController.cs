using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;

namespace QueueManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OwnersController(
    IOwnerRegistrationService ownerRegistrationService,
    ILogger<OwnersController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(OwnerRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OwnerRegistrationResponse>> Register(
        OwnerRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Received owner registration request.");

        var response = await ownerRegistrationService.RegisterAsync(request, cancellationToken);
        return Created($"/api/locations/{response.LocationCode}", response);
    }
}
