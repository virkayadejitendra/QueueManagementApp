using FluentValidation;
using Microsoft.AspNetCore.Identity;
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
builder.Services.AddScoped<IOwnerRegistrationService, OwnerRegistrationService>();
builder.Services.AddScoped<IOwnerRegistrationRepository, OwnerRegistrationRepository>();
builder.Services.AddSingleton<ILocationCodeGenerator, LocationCodeGenerator>();
builder.Services.AddScoped<IValidator<OwnerRegistrationRequest>, OwnerRegistrationRequestValidator>();
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
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();

app.Run();

public partial class Program;
