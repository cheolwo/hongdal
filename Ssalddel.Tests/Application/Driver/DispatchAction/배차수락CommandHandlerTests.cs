using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Driver.DispatchAction;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.공통;
using 살뜰.도메인.운송;
using 살뜰.도메인.화주;

namespace Ssalddel.Tests.Application.Driver.DispatchAction;

public sealed class 배차수락CommandHandlerTests
{
    [Fact]
    public async Task 사후처리Event가실패해도_기사운송진행정보는_배차확정과같이저장된다()
    {
        await using var db = CreateContext();
        var queue = new 운송원장
        {
            의뢰Id = "request-1",
            상태 = 상태값.배차대기상태.대기,
            배차큐단계 = 상태값.배차큐단계.배차추천,
            배차노출상태 = 상태값.배차노출상태.추천중,
            현재추천대상기사Id = "driver-1",
            추천만료시각 = DateTime.UtcNow.AddMinutes(5)
        };
        var request = new 화주운송의뢰
        {
            의뢰Id = "request-1",
            화주Id = "shipper-1",
            주문자UserId = "shipper-1",
            화물종류 = "사과",
            결제상태 = 상태값.결제상태.결제완료,
            배차상태 = 상태값.배차상태.매칭중,
            픽업_도로명주소 = "서울시 강남구",
            픽업_상세주소 = "1층",
            하차_도로명주소 = "서울시 송파구",
            하차_상세주소 = "2층",
            최종운임 = 42000
        };
        db.운송원장.Add(queue);
        db.화주운송의뢰.Add(request);
        await db.SaveChangesAsync();

        var handler = new 배차수락CommandHandler(
            db,
            new ThrowingPublisher(),
            new TestCurrentUserAccessor("driver-1", "기사"),
            new 참여자실행권한검사(),
            new WorkRelationshipSnapshotCollector(),
            NullLogger<배차수락CommandHandler>.Instance);

        var result = await handler.Handle(
            new 배차수락Command("driver-1", "request-1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(상태값.배차대기상태.확정, queue.상태);
        Assert.Equal("driver-1", queue.기사_운송자);
        Assert.Equal("driver-1", queue.확정기사Id);
        Assert.Equal("request-1", queue.운송번호);
        Assert.Equal(request.픽업_도로명주소, queue.픽업_도로명주소);
        Assert.Equal(request.하차_도로명주소, queue.하차_도로명주소);
        Assert.Equal(request.최종운임, queue.운임);
        Assert.Equal(상태값.배차상태.배차확정, request.배차상태);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"dispatch-accept-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;

    private sealed class ThrowingPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("temporary event failure"));

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.FromException(new InvalidOperationException("temporary event failure"));
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
