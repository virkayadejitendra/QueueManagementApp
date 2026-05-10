using Microsoft.EntityFrameworkCore;

namespace QueueManagement.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
