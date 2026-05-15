using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Data;
using QueueManagement.Api.DTOs;
using QueueManagement.Api.Entities;

namespace QueueManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OwnersController(
    AppDbContext dbContext,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    private const string LocationCodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [HttpPost("register")]
    [ProducesResponseType(typeof(OwnerRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OwnerRegistrationResponse>> Register(
        OwnerRegistrationRequest request,
        CancellationToken cancellationToken)
    {
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
            LocationCode = CreateLocationCode()
        };

        var userLocation = new UserLocation
        {
            User = owner,
            QueueLocation = queueLocation,
            Role = UserLocationRoles.Owner
        };

        dbContext.Users.Add(owner);
        dbContext.QueueLocations.Add(queueLocation);
        dbContext.UserLocations.Add(userLocation);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new OwnerRegistrationResponse(
            owner.Id,
            queueLocation.Id,
            queueLocation.LocationCode,
            queueLocation.BusinessName,
            queueLocation.LocationName,
            userLocation.Role);

        return Created($"/api/locations/{queueLocation.LocationCode}", response);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CreateLocationCode()
    {
        Span<char> code = stackalloc char[8];

        for (var index = 0; index < code.Length; index++)
        {
            code[index] = LocationCodeCharacters[
                RandomNumberGenerator.GetInt32(LocationCodeCharacters.Length)];
        }

        return new string(code);
    }

}
