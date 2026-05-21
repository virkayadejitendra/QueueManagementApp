using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.ExternalServices;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Tests;

public sealed class ManagerQueueTests
{
    [Fact]
    public async Task OpenQueue_WithAuthenticatedOwner_MarksQueueOpen()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/manager/queue/open", null);
        var body = await response.Content.ReadFromJsonAsync<ManagerQueueStatusResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.IsQueueOpen);
        Assert.Equal("AB7K2M9Q", body.LocationCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await dbContext.QueueLocations.Select(location => location.IsQueueOpen).SingleAsync());
    }

    [Fact]
    public async Task ResetQueue_WithAuthenticatedOwner_DeletesTodayEntriesAndClosesQueue()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(seedQueueEntry: true);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/manager/queue/reset", null);
        var body = await response.Content.ReadFromJsonAsync<ManagerQueueStatusResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.IsQueueOpen);
        Assert.Equal(0, body.WaitingCount);
        Assert.Null(body.CurrentTokenNumber);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await dbContext.QueueEntries.ToListAsync());
        Assert.False(await dbContext.QueueLocations.Select(location => location.IsQueueOpen).SingleAsync());
    }

    [Fact]
    public async Task OpenQueue_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new QueueManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/manager/queue/open", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record ManagerQueueStatusResponse(
        int QueueLocationId,
        string LocationCode,
        string BusinessName,
        bool IsQueueOpen,
        int WaitingCount,
        int? CurrentTokenNumber);

    private sealed class QueueManagementApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? connection;

        public async Task<string> SeedOwnerAndCreateTokenAsync(bool seedQueueEntry = false)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var owner = new User
            {
                Name = "Priya Sharma",
                Email = "priya@example.com",
                PasswordHash = "not-used-in-this-test"
            };
            var queueLocation = new QueueLocation
            {
                BusinessName = "Priya Dental Clinic",
                Address = "12 MG Road, Bengaluru",
                Mobile = "9876500000",
                LocationCode = "AB7K2M9Q",
                IsQueueOpen = false
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

            if (seedQueueEntry)
            {
                dbContext.QueueEntries.Add(new QueueEntry
                {
                    QueueLocation = queueLocation,
                    CustomerName = "Amit Kumar",
                    BusinessDate = GetBusinessDate(DateTimeOffset.Now, queueLocation.QueueResetTime),
                    TokenNumber = 1,
                    TrackingToken = "tracking-token",
                    Status = QueueEntryStatuses.Waiting
                });
            }

            await dbContext.SaveChangesAsync();

            var jwtTokenService = new JwtTokenService(scope.ServiceProvider.GetRequiredService<IConfiguration>());
            return jwtTokenService.CreateToken(owner, [UserLocationRoles.Owner]).Token;
        }

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

        private static DateOnly GetBusinessDate(DateTimeOffset now, TimeOnly queueResetTime)
        {
            var date = DateOnly.FromDateTime(now.LocalDateTime);
            var time = TimeOnly.FromDateTime(now.LocalDateTime);
            return time < queueResetTime ? date.AddDays(-1) : date;
        }
    }
}
