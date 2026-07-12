using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hongdal.Services.Community;

public sealed class 투영갱신커뮤니티원장저장소 : I커뮤니티원장저장소
{
    private readonly Mongo커뮤니티원장저장소 _inner;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<투영갱신커뮤니티원장저장소> _logger;

    public 투영갱신커뮤니티원장저장소(
        Mongo커뮤니티원장저장소 inner,
        IServiceScopeFactory scopeFactory,
        ILogger<투영갱신커뮤니티원장저장소> logger)
    {
        _inner = inner;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<커뮤니티원장Dto> 원장저장Async(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var 원장 = await _inner.원장저장Async(request, updatedBy, cancellationToken);
        await 원장상태이벤트기록Async(원장, request, updatedBy, cancellationToken);
        await 투영갱신Async(원장, cancellationToken);
        return 원장;
    }

    public Task<커뮤니티원장Dto?> 원장조회Async(
        string 원장Id,
        CancellationToken cancellationToken = default)
        => _inner.원장조회Async(원장Id, cancellationToken);

    public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default)
        => _inner.원장목록조회Async(query, cancellationToken);

    public async Task<커뮤니티원장Dto?> 원장상태변경Async(
        커뮤니티원장상태변경요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var 원장 = await _inner.원장상태변경Async(request, updatedBy, cancellationToken);
        if (원장 is not null)
        {
            await 원장상태변경이벤트기록Async(request, 원장, updatedBy, cancellationToken);
            await 투영갱신Async(원장, cancellationToken);
        }

        return 원장;
    }

    private async Task 투영갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken)
    {
        await 블록관계투영갱신Async(원장, cancellationToken);
        await 업무투영동기화Async(원장, cancellationToken);
    }

    private async Task 원장상태이벤트기록Async(
        커뮤니티원장Dto 원장,
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var stateEventService = scope.ServiceProvider.GetRequiredService<I커뮤니티원장상태이벤트Service>();
            await stateEventService.저장이벤트기록Async(원장, updatedBy, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "커뮤니티 원장 상태 이벤트 기록에 실패했습니다. 원장Id={원장Id}, 요청원장Id={요청원장Id}",
                원장.원장Id,
                request.원장Id);
        }
    }

    private async Task 원장상태변경이벤트기록Async(
        커뮤니티원장상태변경요청 request,
        커뮤니티원장Dto 원장,
        string updatedBy,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var stateEventService = scope.ServiceProvider.GetRequiredService<I커뮤니티원장상태이벤트Service>();
            await stateEventService.상태변경이벤트기록Async(request, 원장, updatedBy, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "커뮤니티 원장 상태 변경 이벤트 기록에 실패했습니다. 원장Id={원장Id}", 원장.원장Id);
        }
    }

    private async Task 블록관계투영갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var blockProjectionService = scope.ServiceProvider.GetRequiredService<I커뮤니티원장블록관계투영Service>();
            await blockProjectionService.갱신Async(원장, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "커뮤니티 원장 블록 관계 투영 갱신에 실패했습니다. 원장Id={원장Id}", 원장.원장Id);
        }
    }

    private async Task 업무투영동기화Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var businessProjectionService = scope.ServiceProvider.GetRequiredService<I커뮤니티원장업무투영동기화Service>();
            await businessProjectionService.갱신Async(원장, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "커뮤니티 원장 업무 투영 동기화에 실패했습니다. 원장Id={원장Id}", 원장.원장Id);
        }
    }
}
