using Microsoft.Extensions.Logging;

namespace Ssalddel.Services.Community;

public interface I커뮤니티원장업무투영동기화Service
{
    Task 갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default);
}

public interface I원장업무투영동기화Handler
{
    bool 처리대상인가(커뮤니티원장Dto 원장);

    Task 동기화Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default);
}

public sealed class 커뮤니티원장업무투영동기화Service : I커뮤니티원장업무투영동기화Service
{
    private readonly IEnumerable<I원장업무투영동기화Handler> _handlers;
    private readonly ILogger<커뮤니티원장업무투영동기화Service> _logger;

    public 커뮤니티원장업무투영동기화Service(
        IEnumerable<I원장업무투영동기화Handler> handlers,
        ILogger<커뮤니티원장업무투영동기화Service> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async Task 갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default)
    {
        var failures = new List<Exception>();
        foreach (var handler in _handlers)
        {
            var handlerName = handler.GetType().Name;
            bool 대상;
            try
            {
                대상 = handler.처리대상인가(원장);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "커뮤니티 원장 업무 투영 대상 판정에 실패했습니다. Handler={Handler}, 원장Id={원장Id}", handlerName, 원장.원장Id);
                failures.Add(ex);
                continue;
            }

            if (!대상)
            {
                continue;
            }

            try
            {
                await handler.동기화Async(원장, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "커뮤니티 원장 업무 투영 동기화에 실패했습니다. Handler={Handler}, 원장Id={원장Id}", handlerName, 원장.원장Id);
                failures.Add(ex);
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException("커뮤니티 원장 업무 투영 중 하나 이상의 처리기가 실패했습니다.", failures);
        }
    }
}
