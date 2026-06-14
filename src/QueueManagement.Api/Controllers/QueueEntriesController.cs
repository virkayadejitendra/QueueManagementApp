using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;

namespace QueueManagement.Api.Controllers;

[ApiController]
[Route("api/queue-entries")]
public sealed class QueueEntriesController(ICustomerQueueService customerQueueService) : ControllerBase
{
    [HttpGet("{queueEntryId:int}/status")]
    [ProducesResponseType(typeof(CustomerQueueStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerQueueStatusResponse>> GetStatus(
        int queueEntryId,
        [FromQuery] string trackingToken,
        CancellationToken cancellationToken)
    {
        var response = await customerQueueService.GetStatusAsync(
            queueEntryId,
            trackingToken,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }
}
