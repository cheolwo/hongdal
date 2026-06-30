using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Hongdal.Application.CommandProcessing;

namespace Hongdal.Application.Behaviors;

public sealed class Command후처리Behavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<ICommand후처리Processor> _processors;
    private readonly ICommand기능설정Resolver _command기능설정Resolver;
    private readonly ILogger<Command후처리Behavior<TRequest, TResponse>> _logger;

    public Command후처리Behavior(
        IEnumerable<ICommand후처리Processor> processors,
        ICommand기능설정Resolver command기능설정Resolver,
        ILogger<Command후처리Behavior<TRequest, TResponse>> logger)
    {
        _processors = processors;
        _command기능설정Resolver = command기능설정Resolver;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (!CommandResult판별기.TryGet성공여부(response, out var isSuccess) || !isSuccess)
        {
            return response;
        }

        var commandName = typeof(TRequest).Name;
        var rule = await _command기능설정Resolver.ResolveAsync(commandName, cancellationToken);
        if (!rule.AuditLogEnabled.GetValueOrDefault()
            && !rule.SmsEnabled.GetValueOrDefault()
            && !rule.SnsEnabled.GetValueOrDefault()
            && !rule.PushEnabled.GetValueOrDefault())
        {
            return response;
        }

        var context = new Command후처리Context(
            commandName,
            request,
            response,
            true,
            Activity.Current?.TraceId.ToString() ?? string.Empty,
            DateTime.UtcNow,
            rule);

        foreach (var processor in _processors)
        {
            if (!processor.CanProcess(rule))
            {
                continue;
            }

            try
            {
                await processor.ProcessAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Command 후처리 실패 Processor={Processor} CommandName={CommandName} TraceId={TraceId}",
                    processor.Name,
                    commandName,
                    context.TraceId);
            }
        }

        return response;
    }
}
