using Application.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Application.Common.Models;
using Newtonsoft.Json.Serialization;
using Npgsql;

namespace Infrastructure.Middlewares;

// ReSharper disable once ClassNeverInstantiated.Global
public class CustomExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<CustomExceptionHandlerMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
#pragma warning disable CA2254 // Template should be a static expression
            logger.LogError(ex, ex.Message);
#pragma warning restore CA2254 // Template should be a static expression
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        var result = string.Empty;
        var errorResponse = new Result
        {
            Success = false,
            Errors = new List<string>()
        };

        if (exception is UnauthorizedAccessException ||
            exception is NotUnauthorizedException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            result = "Unauthorized";
        }
        else if (exception is NotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            if (!string.IsNullOrEmpty(exception.Message))
            {
                result = exception.Message;
            }
        }
        else if (exception.InnerException != null &&
                 exception.InnerException is NpgsqlException sqlException &&
                 sqlException.ErrorCode == 23505)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        }
        else if (exception.InnerException is InvalidConfiguration)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            if (!string.IsNullOrEmpty(exception.InnerException.Message))
            {
                result = JsonConvert.SerializeObject(new { error = exception.InnerException.Message });
            }
        }
        else if (exception is BadRequestException || exception is ApplicationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            result = JsonConvert.SerializeObject(new { error = exception.Message });
        }

#if DEBUG
            if (string.IsNullOrEmpty(result))
            {
                result = JsonConvert.SerializeObject(new { error = exception.ToString() });
            }
#endif
        
        if (!string.IsNullOrEmpty(result))
        {
            errorResponse.Errors.Add(result);
        }

        return context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        }));
    }
}

public static class CustomExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
    }
}