using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Application.Exceptions;

namespace QueueManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DuplicateOwnerContactException exception)
        {
            logger.LogInformation(
                exception,
                "Owner registration conflict while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Owner contact already exists.",
                Detail = exception.Message
            };

            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
        catch (QueueClosedException exception)
        {
            logger.LogInformation(
                exception,
                "Closed queue conflict while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Queue is closed.",
                Detail = exception.Message
            };

            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
        catch (ManagedQueueNotFoundException exception)
        {
            logger.LogInformation(
                exception,
                "Managed queue was not found while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Queue location not found.",
                Detail = exception.Message
            };

            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Please try again later."
            };

            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
    }
}
