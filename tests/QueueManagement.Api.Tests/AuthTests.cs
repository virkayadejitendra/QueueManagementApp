using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Tests;

public sealed class AuthTests
{
    [Fact]
    public async Task Login_WithValidEmailAndPassword_ReturnsBearerToken()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();
        await RegisterOwnerAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            identifier = "PRIYA@example.com",
            password = "StrongPass123"
        });

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(3, body.AccessToken.Split('.').Length);
        Assert.Equal("Bearer", body.TokenType);
        Assert.Equal(3600, body.ExpiresIn);
        Assert.Equal("Priya Sharma", body.Name);
        Assert.Contains(UserLocationRoles.Owner, body.Roles);
    }

    [Fact]
    public async Task Login_WithValidMobileAndPassword_ReturnsBearerToken()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();
        await RegisterOwnerAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            identifier = "9876543210",
            password = "StrongPass123"
        });

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Contains(UserLocationRoles.Owner, body.Roles);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();
        await RegisterOwnerAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            identifier = "priya@example.com",
            password = "WrongPass123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownIdentifier_ReturnsUnauthorized()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            identifier = "missing@example.com",
            password = "StrongPass123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CurrentUser_WithValidToken_ReturnsAuthenticatedUser()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();
        await RegisterOwnerAsync(client);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            identifier = "priya@example.com",
            password = "StrongPass123"
        });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            login!.AccessToken);

        var response = await client.GetAsync("/api/auth/me");
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(login.UserId, body.UserId);
        Assert.Equal("Priya Sharma", body.Name);
        Assert.Contains(UserLocationRoles.Owner, body.Roles);
    }

    private static async Task RegisterOwnerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/owners/register", new
        {
            ownerName = "Priya Sharma",
            email = "priya@example.com",
            mobile = "9876543210",
            password = "StrongPass123",
            businessName = "Priya Dental Clinic",
            locationName = "Main Branch",
            address = "12 MG Road, Bengaluru",
            businessMobile = "9876500000"
        });

        response.EnsureSuccessStatusCode();
    }

    private sealed record LoginResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        int UserId,
        string Name,
        IReadOnlyCollection<string> Roles);

    private sealed record CurrentUserResponse(
        int UserId,
        string Name,
        IReadOnlyCollection<string> Roles);

    private sealed class QueueManagementApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "QueueManagement.Api.Tests",
                    ["Jwt:Audience"] = "QueueManagement.Api.Tests",
                    ["Jwt:SigningKey"] = "test-only-queue-management-signing-key-change-for-production-2026",
                    ["Jwt:ExpiresMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AppDbContext>>();

                connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();

                services.AddDbContext<AppDbContext>(
                    options => options.UseSqlite(connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                connection?.Dispose();
            }
        }
    }
}
