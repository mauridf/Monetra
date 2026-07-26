using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Monetra.Application.Common.Behaviors;

/// <summary>
/// Behavior do MediatR para logging automático de requests/responses.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestType = typeof(TRequest).IsAssignableTo(typeof(IRequest)) ? "Query" : "Command";

        _logger.LogInformation(
            "Handling {RequestType} {RequestName}: {@Request}",
            requestType, requestName, request);

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next();

            sw.Stop();

            _logger.LogInformation(
                "Handled {RequestType} {RequestName} in {ElapsedMs}ms",
                requestType, requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(ex,
                "Error handling {RequestType} {RequestName} after {ElapsedMs}ms",
                requestType, requestName, sw.ElapsedMilliseconds);

            throw;
        }
    }
}
