using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;
        var remoteIp = context.Connection.RemoteIpAddress;
        
        _logger.LogInformation("[{RequestId}] {RequestMethod} {RequestPath} started - From: {RemoteIp}", 
            requestId, requestMethod, requestPath, remoteIp);
        
        try
        {
            await _next(context);
            stopwatch.Stop();
            
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            _logger.LogInformation("[{RequestId}] {RequestMethod} {RequestPath} completed - Status: {StatusCode} in {ElapsedMs}ms",
                requestId, requestMethod, requestPath, statusCode, elapsedMs);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _logger.LogInformation("[{RequestId}] {RequestMethod} {RequestPath} failed after {ElapsedMs}ms",
                requestId, requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}

// Extension method for registering the middleware
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}