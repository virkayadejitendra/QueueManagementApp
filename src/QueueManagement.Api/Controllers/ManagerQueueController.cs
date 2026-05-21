using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;

namespace QueueManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/manager/queue")]
public sealed class ManagerQueueController(IManagerQueueService managerQueueService) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(ManagerQueueStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerQueueStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.GetStatusAsync(GetUserId(), cancellationToken));
    }

    [HttpPost("open")]
    [ProducesResponseType(typeof(ManagerQueueStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerQueueStatusResponse>> Open(CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.OpenAsync(GetUserId(), cancellationToken));
    }

    [HttpPost("close")]
    [ProducesResponseType(typeof(ManagerQueueStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerQueueStatusResponse>> Close(CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.CloseAsync(GetUserId(), cancellationToken));
    }

    [HttpPost("reset")]
    [ProducesResponseType(typeof(ManagerQueueStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerQueueStatusResponse>> Reset(CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.ResetAsync(GetUserId(), cancellationToken));
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
