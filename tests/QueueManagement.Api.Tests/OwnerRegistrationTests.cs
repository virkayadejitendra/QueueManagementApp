using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QueueManagement.Api.Data;
using QueueManagement.Api.Entities;

namespace QueueManagement.Api.Tests;

public sealed class OwnerRegistrationTests
{
    [Fact]
    public async Task RegisterOwner_WithValidRequest_CreatesOwnerLocationAndRole()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();

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

        var body = await response.Content.ReadFromJsonAsync<OwnerRegistrationResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.OwnerId > 0);
        Assert.True(body.QueueLocationId > 0);
        Assert.Equal("Priya Dental Clinic", body.BusinessName);
        Assert.Equal("Main Branch", body.LocationName);
        Assert.Equal(UserLocationRoles.Owner, body.Role);
        Assert.Matches("^[A-Z2-9]{8}$", body.LocationCode);
        Assert.EndsWith($"/api/locations/{body.LocationCode}", response.Headers.Location?.ToString());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var owner = await dbContext.Users.SingleAsync();
        var queueLocation = await dbContext.QueueLocations.SingleAsync();
        var userLocation = await dbContext.UserLocations.SingleAsync();

        Assert.Equal("Priya Sharma", owner.Name);
        Assert.Equal("priya@example.com", owner.Email);
        Assert.NotEqual("StrongPass123", owner.PasswordHash);
        Assert.StartsWith("AQAAAA", owner.PasswordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<User>().VerifyHashedPassword(owner, owner.PasswordHash, "StrongPass123"));
        Assert.Equal("Priya Dental Clinic", queueLocation.BusinessName);
        Assert.Equal(body.LocationCode, queueLocation.LocationCode);
        Assert.Equal(owner.Id, userLocation.UserId);
        Assert.Equal(queueLocation.Id, userLocation.QueueLocationId);
        Assert.Equal(UserLocationRoles.Owner, userLocation.Role);
    }

    [Fact]
    public async Task RegisterOwner_WithoutEmailOrMobile_ReturnsValidationProblem()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/owners/register", new
        {
            ownerName = "Priya Sharma",
            password = "StrongPass123",
            businessName = "Priya Dental Clinic",
            address = "12 MG Road, Bengaluru",
            businessMobile = "9876500000"
        });

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.GetProperty("errors").TryGetProperty("Email", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("Mobile", out _));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await dbContext.Users.ToListAsync());
        Assert.Empty(await dbContext.QueueLocations.ToListAsync());
        Assert.Empty(await dbContext.UserLocations.ToListAsync());
    }

    [Fact]
    public async Task RegisterOwner_WithWhitespaceRequiredFields_ReturnsValidationProblem()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/owners/register", new
        {
            ownerName = "   ",
            email = "priya@example.com",
            password = "StrongPass123",
            businessName = "   ",
            address = "   ",
            businessMobile = "   "
        });

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());

        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("OwnerName", out _));
        Assert.True(errors.TryGetProperty("BusinessName", out _));
        Assert.True(errors.TryGetProperty("Address", out _));
        Assert.True(errors.TryGetProperty("BusinessMobile", out _));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await dbContext.Users.ToListAsync());
        Assert.Empty(await dbContext.QueueLocations.ToListAsync());
        Assert.Empty(await dbContext.UserLocations.ToListAsync());
    }

    private sealed record OwnerRegistrationResponse(
        int OwnerId,
        int QueueLocationId,
        string LocationCode,
        string BusinessName,
        string? LocationName,
        string Role);

    private sealed class QueueManagementApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    service => service.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbContextDescriptor is not null)
                {
                    services.Remove(dbContextDescriptor);
                }

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
