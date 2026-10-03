using System.Net;
using System.Text.Json;
using DigitalMemoryMap.BLL.Exceptions;

namespace DigitalMemoryMap.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle 401 / 403 status codes produced directly by ASP.NET Authorization pipeline
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = 401,
                    message = "Please log in."
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = 403,
                    message = "You do not have permission."
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Something went wrong. Please try again.";
        IDictionary<string, string[]>? errors = null;

        if (exception is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            message = appEx.Message;
            errors = appEx.Errors;
        }

        context.Response.StatusCode = statusCode;

        var responseObj = new Dictionary<string, object>
        {
            ["status"] = statusCode,
            ["message"] = message
        };

        if (errors != null && errors.Count > 0)
        {
            responseObj["errors"] = errors;
        }

        var json = JsonSerializer.Serialize(responseObj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
