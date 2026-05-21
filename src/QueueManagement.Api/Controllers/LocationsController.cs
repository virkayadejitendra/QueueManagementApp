using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;

namespace QueueManagement.Api.Controllers;

[ApiController]
[Route("api/locations")]
public sealed class LocationsController(ICustomerQueueService customerQueueService) : ControllerBase
{
    [HttpPost("{locationCode}/queue-entries")]
    [ProducesResponseType(typeof(CustomerJoinQueueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerJoinQueueResponse>> JoinQueue(
        string locationCode,
        CustomerJoinQueueRequest request,
        CancellationToken cancellationToken)
    {
        var response = await customerQueueService.JoinAsync(
            locationCode,
            request,
            cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return Created(response.StatusUrl, response);
    }
}
