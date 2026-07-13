using Hongdal.Application.Community.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Services.Community;

public sealed class 이벤트발행커뮤니티원장저장소 : I커뮤니티원장저장소
{
    private readonly Mongo커뮤니티원장저장소 _inner;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<이벤트발행커뮤니티원장저장소> _logger;

    public 이벤트발행커뮤니티원장저장소(
        Mongo커뮤니티원장저장소 inner,
        IServiceScopeFactory scopeFactory,
        ILogger<이벤트발행커뮤니티원장저장소> logger)
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
        await 변경이벤트발행Async(
            원장,
            커뮤니티원장변경유형.저장,
            updatedBy,
            상태변경요청: null,
            cancellationToken);
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
            await 변경이벤트발행Async(
                원장,
                커뮤니티원장변경유형.상태변경,
                updatedBy,
                request,
                cancellationToken);
        }

        return 원장;
    }

    private async Task 변경이벤트발행Async(
        커뮤니티원장Dto 원장,
        string 변경유형,
        string updatedBy,
        커뮤니티원장상태변경요청? 상태변경요청,
        CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid().ToString("N");

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            await publisher.Publish(
                new 커뮤니티원장변경됨Event(
                    원장,
                    변경유형,
                    string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
                    상태변경요청,
                    DateTime.UtcNow,
                    eventId),
                cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "커뮤니티 원장 변경 이벤트 발행에 실패했습니다. EventId={EventId}, 원장Id={원장Id}, 변경유형={변경유형}",
                eventId,
                원장.원장Id,
                변경유형);
        }
    }
}
