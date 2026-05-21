using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QueueManagement.Api.Application.DTOs;
using QueueManagement.Api.Application.Interfaces;
using QueueManagement.Api.Application.Services;
using QueueManagement.Api.Application.Validators;
using QueueManagement.Api.Domain.Entities;
using QueueManagement.Api.Filters;
using QueueManagement.Api.Infrastructure.ExternalServices;
using QueueManagement.Api.Infrastructure.Persistence;
using QueueManagement.Api.Infrastructure.Repositories;
using QueueManagement.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationActionFilter>();
});
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICustomerQueueService, CustomerQueueService>();
builder.Services.AddScoped<ICustomerQueueRepository, CustomerQueueRepository>();
builder.Services.AddScoped<IManagerQueueService, ManagerQueueService>();
builder.Services.AddScoped<IManagerQueueRepository, ManagerQueueRepository>();
builder.Services.AddScoped<IOwnerRegistrationService, OwnerRegistrationService>();
builder.Services.AddScoped<IOwnerRegistrationRepository, OwnerRegistrationRepository>();
builder.Services.AddSingleton<ILocationCodeGenerator, LocationCodeGenerator>();
builder.Services.AddScoped<IValidator<CustomerJoinQueueRequest>, CustomerJoinQueueRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<OwnerRegistrationRequest>, OwnerRegistrationRequestValidator>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var signingKey = builder.Configuration["Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSqlite<AppDbContext>(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=queue-management.db");
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "QueueEntries" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_QueueEntries" PRIMARY KEY AUTOINCREMENT,
            "QueueLocationId" INTEGER NOT NULL,
            "CustomerName" TEXT NOT NULL,
            "Mobile" TEXT NULL,
            "PartySize" INTEGER NULL,
            "ServiceReason" TEXT NULL,
            "BusinessDate" TEXT NOT NULL,
            "TokenNumber" INTEGER NOT NULL,
            "TrackingToken" TEXT NOT NULL,
            "Status" TEXT NOT NULL,
            "CreatedAt" TEXT NOT NULL,
            CONSTRAINT "FK_QueueEntries_QueueLocations_QueueLocationId"
                FOREIGN KEY ("QueueLocationId") REFERENCES "QueueLocations" ("Id") ON DELETE CASCADE
        );
        """);
    dbContext.Database.ExecuteSqlRaw("""
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_QueueEntries_QueueLocationId_BusinessDate_TokenNumber"
        ON "QueueEntries" ("QueueLocationId", "BusinessDate", "TokenNumber");
        """);
    dbContext.Database.ExecuteSqlRaw("""
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_QueueEntries_TrackingToken"
        ON "QueueEntries" ("TrackingToken");
        """);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
