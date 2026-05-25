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

    [Fact]
    public async Task CallNext_WhenQueueIsOpenAndWaitingEntryExists_MarksEntryCalled()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(isQueueOpen: true);
        await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Waiting, tokenNumber: 1);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/manager/queue/call-next", null);
        var body = await response.Content.ReadFromJsonAsync<ManagerQueueTodayResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotNull(body.CurrentCalled);
        Assert.Equal(1, body.CurrentCalled.TokenNumber);
        Assert.Equal(QueueEntryStatuses.Called, body.CurrentCalled.Status);
        Assert.Empty(body.WaitingEntries);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entry = await dbContext.QueueEntries.SingleAsync();
        Assert.Equal(QueueEntryStatuses.Called, entry.Status);
        Assert.Equal(1, entry.CallCount);
        Assert.NotNull(entry.CalledAt);
    }

    [Fact]
    public async Task CallNext_WhenCustomerAlreadyCalled_ReturnsConflict()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(isQueueOpen: true);
        await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Called, tokenNumber: 1);
        await factory.SeedQueueEntryAsync("Neha Rao", QueueEntryStatuses.Waiting, tokenNumber: 2);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/manager/queue/call-next", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var waitingEntry = await dbContext.QueueEntries.SingleAsync(entry => entry.TokenNumber == 2);
        Assert.Equal(QueueEntryStatuses.Waiting, waitingEntry.Status);
    }

    [Fact]
    public async Task WalkIn_WhenQueueIsOpen_CreatesNextWaitingToken()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(isQueueOpen: true);
        await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Waiting, tokenNumber: 1);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/manager/queue/walk-in", new
        {
            customerName = "Neha Rao",
            partySize = 2,
            serviceReason = "Consultation"
        });
        var body = await response.Content.ReadFromJsonAsync<ManagerQueueTodayResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(2, body.WaitingCount);
        Assert.Contains(body.WaitingEntries, entry => entry.TokenNumber == 2 && entry.CustomerName == "Neha Rao");
    }

    [Fact]
    public async Task Served_WhenEntryIsCalled_ClearsCurrentCalledCustomer()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(isQueueOpen: true);
        var queueEntryId = await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Called, tokenNumber: 1);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"/api/manager/queue/entries/{queueEntryId}/served", null);
        var body = await response.Content.ReadFromJsonAsync<ManagerQueueTodayResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Null(body.CurrentCalled);
        Assert.Contains(body.RecentServedEntries, entry => entry.QueueEntryId == queueEntryId);
    }

    [Fact]
    public async Task SkippedEntry_IsExcludedFromCallNextUntilRestored()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(isQueueOpen: true);
        var skippedEntryId = await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Skipped, tokenNumber: 1);
        await factory.SeedQueueEntryAsync("Neha Rao", QueueEntryStatuses.Waiting, tokenNumber: 2);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var callNextResponse = await client.PostAsync("/api/manager/queue/call-next", null);
        var callNextBody = await callNextResponse.Content.ReadFromJsonAsync<ManagerQueueTodayResponse>();

        Assert.Equal(HttpStatusCode.OK, callNextResponse.StatusCode);
        Assert.NotNull(callNextBody?.CurrentCalled);
        Assert.Equal(2, callNextBody.CurrentCalled.TokenNumber);

        await client.PostAsync($"/api/manager/queue/entries/{callNextBody.CurrentCalled.QueueEntryId}/served", null);
        var restoreResponse = await client.PostAsync($"/api/manager/queue/entries/{skippedEntryId}/restore", null);
        var restoreBody = await restoreResponse.Content.ReadFromJsonAsync<ManagerQueueTodayResponse>();

        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);
        Assert.Contains(restoreBody!.WaitingEntries, entry => entry.QueueEntryId == skippedEntryId);
    }

    [Fact]
    public async Task CloseQueue_CancelsActiveEntriesAndPreventsCustomerJoin()
    {
        using var factory = new QueueManagementApiFactory();
        var token = await factory.SeedOwnerAndCreateTokenAsync(isQueueOpen: true);
        await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Waiting, tokenNumber: 1);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var closeResponse = await client.PostAsync("/api/manager/queue/close", null);
        client.DefaultRequestHeaders.Authorization = null;
        var joinResponse = await client.PostAsJsonAsync("/api/locations/AB7K2M9Q/queue-entries", new
        {
            customerName = "Neha Rao"
        });

        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, joinResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(QueueEntryStatuses.Cancelled, (await dbContext.QueueEntries.SingleAsync()).Status);
    }

    private sealed record ManagerQueueStatusResponse(
        int QueueLocationId,
        string LocationCode,
        string BusinessName,
        bool IsQueueOpen,
        int WaitingCount,
        int? CurrentTokenNumber);

    private sealed record ManagerQueueTodayResponse(
        int QueueLocationId,
        string LocationCode,
        string BusinessName,
        bool IsQueueOpen,
        int WaitingCount,
        ManagerQueueEntryResponse? CurrentCalled,
        IReadOnlyList<ManagerQueueEntryResponse> WaitingEntries,
        IReadOnlyList<ManagerQueueEntryResponse> SkippedEntries,
        IReadOnlyList<ManagerQueueEntryResponse> RecentServedEntries);

    private sealed record ManagerQueueEntryResponse(
        int QueueEntryId,
        int TokenNumber,
        string CustomerName,
        string? Mobile,
        int? PartySize,
        string? ServiceReason,
        string Status,
        int CallCount,
        int SkipCount);

    private sealed class QueueManagementApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? connection;

        public async Task<string> SeedOwnerAndCreateTokenAsync(
            bool seedQueueEntry = false,
            bool isQueueOpen = false)
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
                IsQueueOpen = isQueueOpen
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
                    Status = QueueEntryStatuses.Waiting,
                    SortOrder = 1
                });
            }

            await dbContext.SaveChangesAsync();

            var jwtTokenService = new JwtTokenService(scope.ServiceProvider.GetRequiredService<IConfiguration>());
            return jwtTokenService.CreateToken(owner, [UserLocationRoles.Owner]).Token;
        }

        public async Task<int> SeedQueueEntryAsync(
            string customerName,
            string status,
            int tokenNumber)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var queueLocation = await dbContext.QueueLocations.SingleAsync();
            var queueEntry = new QueueEntry
            {
                QueueLocationId = queueLocation.Id,
                CustomerName = customerName,
                BusinessDate = GetBusinessDate(DateTimeOffset.Now, queueLocation.QueueResetTime),
                TokenNumber = tokenNumber,
                TrackingToken = $"tracking-token-{tokenNumber}",
                Status = status,
                SortOrder = tokenNumber,
                CallCount = status == QueueEntryStatuses.Called ? 1 : 0,
                CalledAt = status == QueueEntryStatuses.Called ? DateTimeOffset.UtcNow : null,
                LastSkippedAt = status == QueueEntryStatuses.Skipped ? DateTimeOffset.UtcNow : null
            };

            dbContext.QueueEntries.Add(queueEntry);
            await dbContext.SaveChangesAsync();

            return queueEntry.Id;
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
