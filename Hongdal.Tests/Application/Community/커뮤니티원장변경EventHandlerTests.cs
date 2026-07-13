using Hongdal.Application.Community.Events;
using Hongdal.Application.Community.Handlers;
using Hongdal.Services.Community;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hongdal.Tests.Application.Community;

public sealed class 커뮤니티원장변경EventHandlerTests
{
    [Fact]
    public async Task 원장저장Event_Rdb상태블록업무투영을각각호출한다()
    {
        var 상태Service = new Fake원장상태이벤트Service();
        var 블록Service = new Fake블록관계투영Service();
        var 업무Service = new Fake업무투영Service();
        using var services = BuildServices(상태Service, 블록Service, 업무Service);
        var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
        var notification = CreateEvent(커뮤니티원장변경유형.저장);

        await new 커뮤니티원장상태기록EventHandler(
                scopeFactory,
                NullLogger<커뮤니티원장상태기록EventHandler>.Instance)
            .Handle(notification, CancellationToken.None);
        await new 커뮤니티원장블록관계투영EventHandler(
                scopeFactory,
                NullLogger<커뮤니티원장블록관계투영EventHandler>.Instance)
            .Handle(notification, CancellationToken.None);
        await new 커뮤니티원장업무투영EventHandler(
                scopeFactory,
                NullLogger<커뮤니티원장업무투영EventHandler>.Instance)
            .Handle(notification, CancellationToken.None);

        Assert.Equal(1, 상태Service.저장호출수);
        Assert.Equal(0, 상태Service.상태변경호출수);
        Assert.Equal("ledger-1", Assert.Single(블록Service.원장목록).원장Id);
        Assert.Equal("ledger-1", Assert.Single(업무Service.원장목록).원장Id);
    }

    [Fact]
    public async Task 원장상태변경Event_상태변경요청을Rdb이력에전달한다()
    {
        var 상태Service = new Fake원장상태이벤트Service();
        using var services = BuildServices(
            상태Service,
            new Fake블록관계투영Service(),
            new Fake업무투영Service());
        var handler = new 커뮤니티원장상태기록EventHandler(
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<커뮤니티원장상태기록EventHandler>.Instance);
        var notification = CreateEvent(커뮤니티원장변경유형.상태변경);

        await handler.Handle(notification, CancellationToken.None);

        Assert.Equal(0, 상태Service.저장호출수);
        Assert.Equal(1, 상태Service.상태변경호출수);
        Assert.Equal("완료", 상태Service.마지막상태변경요청?.상태);
    }

    private static ServiceProvider BuildServices(
        I커뮤니티원장상태이벤트Service 상태Service,
        I커뮤니티원장블록관계투영Service 블록Service,
        I커뮤니티원장업무투영동기화Service 업무Service)
        => new ServiceCollection()
            .AddSingleton(상태Service)
            .AddSingleton(블록Service)
            .AddSingleton(업무Service)
            .BuildServiceProvider();

    private static 커뮤니티원장변경됨Event CreateEvent(string 변경유형)
    {
        var 상태변경요청 = 변경유형 == 커뮤니티원장변경유형.상태변경
            ? new 커뮤니티원장상태변경요청
            {
                원장Id = "ledger-1",
                이전상태 = "진행중",
                상태 = "완료",
                현재단계Key = "delivered"
            }
            : null;

        return new 커뮤니티원장변경됨Event(
            new 커뮤니티원장Dto
            {
                원장Id = "ledger-1",
                커뮤니티Id = "platform",
                원장템플릿Key = "cargo-transport",
                제목 = "테스트 원장",
                상태 = 상태변경요청?.상태 ?? "초안"
            },
            변경유형,
            "user-1",
            상태변경요청,
            DateTime.UtcNow,
            "event-1");
    }

    private sealed class Fake원장상태이벤트Service : I커뮤니티원장상태이벤트Service
    {
        public int 저장호출수 { get; private set; }
        public int 상태변경호출수 { get; private set; }
        public 커뮤니티원장상태변경요청? 마지막상태변경요청 { get; private set; }

        public Task 저장이벤트기록Async(
            커뮤니티원장Dto 원장,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            저장호출수++;
            return Task.CompletedTask;
        }

        public Task 상태변경이벤트기록Async(
            커뮤니티원장상태변경요청 request,
            커뮤니티원장Dto 원장,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            상태변경호출수++;
            마지막상태변경요청 = request;
            return Task.CompletedTask;
        }
    }

    private sealed class Fake블록관계투영Service : I커뮤니티원장블록관계투영Service
    {
        public List<커뮤니티원장Dto> 원장목록 { get; } = [];

        public Task 갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default)
        {
            원장목록.Add(원장);
            return Task.CompletedTask;
        }
    }

    private sealed class Fake업무투영Service : I커뮤니티원장업무투영동기화Service
    {
        public List<커뮤니티원장Dto> 원장목록 { get; } = [];

        public Task 갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default)
        {
            원장목록.Add(원장);
            return Task.CompletedTask;
        }
    }
}
