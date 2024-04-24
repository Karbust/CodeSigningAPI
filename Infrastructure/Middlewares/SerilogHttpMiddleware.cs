using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace Infrastructure.Middlewares;

public class SerilogHttpMiddleware(RequestDelegate next)
{
    public const string MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed} ms";

    private static readonly ILogger _log
        = Serilog.Log.ForContext<SerilogHttpMiddleware>();

    private static readonly HashSet<string> _headerWhitelist
        = new()
            { "Content-Type", "Content-Length", "User-Agent" };

    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(httpContext);

            var statusCode = httpContext.Response?.StatusCode;
            var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

            var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : _log;

            if (level == LogEventLevel.Information && stopwatch.ElapsedMilliseconds > 1500)
            {
                level = LogEventLevel.Warning;
            }

            log.Write(level, MessageTemplate, httpContext.Request.Method, GetPath(httpContext), statusCode, stopwatch.ElapsedMilliseconds);
        }
        // Never caught, because `LogException()` returns false.
        catch (Exception ex) when (LogException(httpContext, stopwatch.ElapsedMilliseconds, ex)) { }
    }

    private static bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
    {
        LogForErrorContext(httpContext)
            .Error(ex, MessageTemplate, httpContext.Request.Method, GetPath(httpContext), 500, elapsedMs);

        return false;
    }

    private static ILogger LogForErrorContext(HttpContext httpContext)
    {
        var request = httpContext.Request;

        var loggedHeaders = request.Headers
            .Where(h => _headerWhitelist.Contains(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        var result = _log
            .ForContext("RequestHeaders", loggedHeaders, destructureObjects: true)
            .ForContext("RequestHost", request.Host)
            .ForContext("RequestProtocol", request.Protocol);

        return result;
    }

    private static string GetPath(HttpContext httpContext)
    {
        return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
    }
}