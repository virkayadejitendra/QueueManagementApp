using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Domain.Entities;

namespace QueueManagement.Api.Application.Services;

public sealed class AuthService(
    IAuthRepository authRepository,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var identifier = NormalizeIdentifier(request.Identifier);
        var user = await authRepository.FindActiveUserByIdentifierAsync(identifier, cancellationToken);

        if (user is null)
        {
            logger.LogInformation("Login failed for unknown identifier.");
            return null;
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            logger.LogInformation("Login failed for user {UserId}.", user.Id);
            return null;
        }

        var roles = user.UserLocations
            .Select(userLocation => userLocation.Role)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        var token = jwtTokenService.CreateToken(user, roles);

        return new LoginResponse(
            token.Token,
            "Bearer",
            token.ExpiresIn,
            user.Id,
            user.Name,
            roles);
    }

    private static string NormalizeIdentifier(string identifier)
    {
        var trimmedIdentifier = identifier.Trim();
        return trimmedIdentifier.Contains('@', StringComparison.Ordinal)
            ? trimmedIdentifier.ToLowerInvariant()
            : trimmedIdentifier;
    }
}
