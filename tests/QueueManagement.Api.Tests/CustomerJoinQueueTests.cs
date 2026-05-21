using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QueueManagement.Api.Domain.BusinessRules;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Infrastructure.Persistence;

namespace QueueManagement.Api.Tests;

public sealed class CustomerJoinQueueTests
{
    [Fact]
    public async Task JoinQueue_WhenQueueIsOpen_CreatesWaitingQueueEntry()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/locations/AB7K2M9Q/queue-entries", new
        {
            customerName = "Amit Kumar",
            mobile = "9876543210",
            partySize = 2,
            serviceReason = "Consultation"
        });

        var body = await response.Content.ReadFromJsonAsync<CustomerJoinQueueResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.QueueEntryId > 0);
        Assert.True(body.QueueLocationId > 0);
        Assert.Equal("AB7K2M9Q", body.LocationCode);
        Assert.Equal(1, body.TokenNumber);
        Assert.Equal(QueueEntryStatuses.Waiting, body.Status);
        Assert.False(string.IsNullOrWhiteSpace(body.TrackingToken));
        Assert.Equal($"/status/{body.QueueEntryId}/{body.TrackingToken}", body.StatusUrl);
        Assert.EndsWith(body.StatusUrl, response.Headers.Location?.ToString());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var queueEntry = await dbContext.QueueEntries.SingleAsync();

        Assert.Equal(body.QueueEntryId, queueEntry.Id);
        Assert.Equal(body.QueueLocationId, queueEntry.QueueLocationId);
        Assert.Equal("Amit Kumar", queueEntry.CustomerName);
        Assert.Equal("9876543210", queueEntry.Mobile);
        Assert.Equal(2, queueEntry.PartySize);
        Assert.Equal("Consultation", queueEntry.ServiceReason);
        Assert.Equal(1, queueEntry.TokenNumber);
        Assert.Equal(body.TrackingToken, queueEntry.TrackingToken);
        Assert.Equal(QueueEntryStatuses.Waiting, queueEntry.Status);
    }

    [Fact]
    public async Task JoinQueue_WhenQueueIsClosed_ReturnsConflict()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: false);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/locations/AB7K2M9Q/queue-entries", new
        {
            customerName = "Amit Kumar"
        });

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.Equal("Queue is closed.", problem.GetProperty("title").GetString());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await dbContext.QueueEntries.ToListAsync());
    }

    [Fact]
    public async Task JoinQueue_WithBlankCustomerName_ReturnsValidationProblem()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/locations/AB7K2M9Q/queue-entries", new
        {
            customerName = "   "
        });

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.GetProperty("errors").TryGetProperty("CustomerName", out _));
    }

    [Fact]
    public async Task JoinQueue_WithInvalidPartySize_ReturnsValidationProblem()
    {
        using var factory = new QueueManagementApiFactory();
        await factory.SeedLocationAsync("AB7K2M9Q", isQueueOpen: true);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/locations/AB7K2M9Q/queue-entries", new
        {
            customerName = "Amit Kumar",
            partySize = 0
        });

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.GetProperty("errors").TryGetProperty("PartySize", out _));
    }

    private sealed record CustomerJoinQueueResponse(
        int QueueEntryId,
        int QueueLocationId,
        string LocationCode,
        int TokenNumber,
        string Status,
        string TrackingToken,
        string StatusUrl);

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
