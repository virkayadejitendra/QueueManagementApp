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
        // Email login should be case-insensitive, but mobile login should keep the user's exact digits/format.
        var identifier = NormalizeIdentifier(request.Identifier);
        var user = await authRepository.FindActiveUserByIdentifierAsync(identifier, cancellationToken);

        // Return null instead of explaining which part failed so attackers cannot discover valid accounts.
        if (user is null)
        {
            logger.LogInformation("Login failed for unknown identifier.");
            return null;
        }

        // PasswordHasher verifies the stored salted hash; the plain password is never stored or compared directly.
        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        // Keep the public API response the same for unknown users and wrong passwords.
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            logger.LogInformation("Login failed for user {UserId}.", user.Id);
            return null;
        }

        // A user can belong to one or more locations, so collect their distinct roles for JWT role claims.
        var roles = user.UserLocations
            .Select(userLocation => userLocation.Role)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        // Token creation is isolated so this service owns login rules, while JWT formatting stays infrastructure code.
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
