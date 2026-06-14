using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Tests;

public sealed class CustomerStatusDisplayTests
{
    [Fact]
    public async Task GetStatus_WithValidTrackingToken_ReturnsPrivateQueueStatus()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        await factory.SeedQueueEntryAsync("Called Customer", QueueEntryStatuses.Called, 1);
        await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Waiting, 2);
        var queueEntryId = await factory.SeedQueueEntryAsync("Neha Rao", QueueEntryStatuses.Waiting, 3);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/queue-entries/{queueEntryId}/status?trackingToken=tracking-token-3");
        var body = await response.Content.ReadFromJsonAsync<CustomerQueueStatusResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(queueEntryId, body.QueueEntryId);
        Assert.Equal("AB7K2M9Q", body.LocationCode);
        Assert.Equal("Priya Dental Clinic", body.BusinessName);
        Assert.Equal(3, body.TokenNumber);
        Assert.Equal(QueueEntryStatuses.Waiting, body.Status);
        Assert.Equal(2, body.QueuePosition);
        Assert.Equal(2, body.WaitingCount);
        Assert.Equal(1, body.CurrentCalledTokenNumber);
    }

    [Fact]
    public async Task GetStatus_WithInvalidTrackingToken_ReturnsNotFound()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        var queueEntryId = await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Waiting, 1);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/queue-entries/{queueEntryId}/status?trackingToken=wrong-token");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDisplay_ReturnsCurrentCalledAndLastServedTokens()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        await factory.SeedQueueEntryAsync("Amit Kumar", QueueEntryStatuses.Called, 1);
        await factory.SeedQueueEntryAsync("Neha Rao", QueueEntryStatuses.Waiting, 2);
        await factory.SeedQueueEntryAsync("Ravi Mehta", QueueEntryStatuses.Served, 3);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/locations/AB7K2M9Q/display");
        var body = await response.Content.ReadFromJsonAsync<QueueDisplayResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("AB7K2M9Q", body.LocationCode);
        Assert.Equal("Priya Dental Clinic", body.BusinessName);
        Assert.True(body.IsQueueOpen);
        Assert.Equal(1, body.CurrentCalledTokenNumber);
        Assert.Equal("Amit Kumar", body.CurrentCalledCustomerName);
        Assert.Equal(3, body.LastServedTokenNumber);
        Assert.Equal("Ravi Mehta", body.LastServedCustomerName);
        Assert.Equal(1, body.WaitingCount);
    }

    [Fact]
    public async Task GetDisplay_WithUnknownLocationCode_ReturnsNotFound()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/locations/UNKNOWN/display");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record CustomerQueueStatusResponse(
        int QueueEntryId,
        string LocationCode,
        string BusinessName,
        int TokenNumber,
        string Status,
        int? QueuePosition,
        int WaitingCount,
        int? CurrentCalledTokenNumber);

    private sealed record QueueDisplayResponse(
        string LocationCode,
        string BusinessName,
        bool IsQueueOpen,
        int? CurrentCalledTokenNumber,
        string? CurrentCalledCustomerName,
        int? LastServedTokenNumber,
        string? LastServedCustomerName,
        int WaitingCount);

    private sealed class QueueManagementApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? connection;

        public async Task SeedLocationAsync(string locationCode, bool isQueueOpen)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.QueueLocations.Add(new QueueLocation
            {
                BusinessName = "Priya Dental Clinic",
                Address = "12 MG Road, Bengaluru",
                Mobile = "9876500000",
                LocationCode = locationCode,
                IsQueueOpen = isQueueOpen
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task<int> SeedQueueEntryAsync(
            string customerName,
            string status,
            int tokenNumber)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var queueLocation = await dbContext.QueueLocations.SingleAsync();
            var now = DateTimeOffset.UtcNow;
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
                CalledAt = status == QueueEntryStatuses.Called ? now : null,
                ServedAt = status == QueueEntryStatuses.Served ? now.AddMinutes(tokenNumber) : null
            };

            dbContext.QueueEntries.Add(queueEntry);
            await dbContext.SaveChangesAsync();

            return queueEntry.Id;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
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

        private static DateOnly GetBusinessDate(DateTimeOffset now, TimeOnly queueResetTime)
        {
            var date = DateOnly.FromDateTime(now.LocalDateTime);
            var time = TimeOnly.FromDateTime(now.LocalDateTime);
            return time < queueResetTime ? date.AddDays(-1) : date;
        }
    }
}
