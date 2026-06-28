using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
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
        var traceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("CQRS request started RequestName={RequestName} TraceId={TraceId}", requestName, traceId);

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "CQRS request completed RequestName={RequestName} TraceId={TraceId} ElapsedMs={ElapsedMs}",
                requestName,
                traceId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "CQRS request failed RequestName={RequestName} TraceId={TraceId} ElapsedMs={ElapsedMs}",
                requestName,
                traceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
