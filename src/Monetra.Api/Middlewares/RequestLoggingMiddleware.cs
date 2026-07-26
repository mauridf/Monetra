using System.Diagnostics;

namespace Monetra.Api.Middlewares;

/// <summary>
/// Middleware para logging detalhado de requests HTTP.
/// </summary>
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
        var sw = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;

        // Adicionar correlation ID ao header da resposta
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            return Task.CompletedTask;
        });

        try
        {
            _logger.LogInformation(
                "[{CorrelationId}] HTTP {Method} {Path} iniciado",
                correlationId,
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            sw.Stop();

            _logger.LogInformation(
                "[{CorrelationId}] HTTP {Method} {Path} finalizado - Status: {StatusCode} em {ElapsedMs}ms",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(ex,
                "[{CorrelationId}] HTTP {Method} {Path} falhou após {ElapsedMs}ms",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds);

            throw;
        }
    }
}
