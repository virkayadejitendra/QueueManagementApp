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

    [HttpGet("today")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> GetToday(CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.GetTodayAsync(GetUserId(), cancellationToken));
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

    [HttpPost("walk-in")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> AddWalkIn(
        ManagerWalkInRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.AddWalkInAsync(GetUserId(), request, cancellationToken));
    }

    [HttpPost("call-next")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> CallNext(CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.CallNextAsync(GetUserId(), cancellationToken));
    }

    [HttpPost("entries/{queueEntryId:int}/served")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> MarkServed(
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.MarkServedAsync(GetUserId(), queueEntryId, cancellationToken));
    }

    [HttpPost("entries/{queueEntryId:int}/no-response")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> MarkNoResponse(
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.MarkNoResponseAsync(GetUserId(), queueEntryId, cancellationToken));
    }

    [HttpPost("entries/{queueEntryId:int}/skipped")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> MarkSkipped(
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.MarkSkippedAsync(GetUserId(), queueEntryId, cancellationToken));
    }

    [HttpPost("entries/{queueEntryId:int}/restore")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> Restore(
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.RestoreAsync(GetUserId(), queueEntryId, cancellationToken));
    }

    [HttpPost("entries/{queueEntryId:int}/cancel")]
    [ProducesResponseType(typeof(ManagerQueueTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManagerQueueTodayResponse>> Cancel(
        int queueEntryId,
        CancellationToken cancellationToken)
    {
        return Ok(await managerQueueService.CancelAsync(GetUserId(), queueEntryId, cancellationToken));
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
