using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Shipper.Request;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.공통;
using 살뜰.도메인.기사;
using 살뜰.도메인.운송;
using 살뜰.도메인.화주;

namespace Ssalddel.Tests.Application.Shipper.Request;

public sealed class 화주운송의뢰조회QueryHandlerTests
{
    [Fact]
    public async Task 소유자단건조회는_확정기사와진행중최근위치를_같이반환한다()
    {
        await using var db = CreateContext();
        var recordedAt = DateTime.UtcNow.AddMinutes(-1);
        db.화주운송의뢰.Add(CreateRequest());
        db.운송원장.Add(new 운송원장
        {
            의뢰Id = "request-1",
            운송번호 = "transport-1",
            확정기사Id = "driver-1",
            기사_운송자 = "driver-1",
            상태 = 상태값.배차상태.운송중,
            UpdatedAt = DateTime.UtcNow
        });
        db.용달기사.Add(new 용달기사
        {
            기사Id = "driver-1",
            기사명 = "안전기사",
            차량 = "1톤 카고"
        });
        db.기사위치기록.Add(new 기사위치기록
        {
            기사Id = "driver-1",
            위도 = 37.501m,
            경도 = 127.039m,
            기록시각 = recordedAt
        });
        await db.SaveChangesAsync();

        var handler = new 의뢰단건조회QueryHandler(
            db,
            new TestCurrentUserAccessor("shipper-1", "화주"));

        var response = await handler.Handle(
            new 의뢰단건조회Query("request-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(상태값.배차상태.운송중, response!.운송상태);
        Assert.Equal("driver-1", response.확정기사Id);
        Assert.Equal("안전기사", response.확정기사명);
        Assert.Equal("1톤 카고", response.확정기사차량);
        Assert.Equal(37.501m, response.기사최근위도);
        Assert.Equal(127.039m, response.기사최근경도);
        Assert.Equal(recordedAt, response.기사최근위치시각Utc);
    }

    [Fact]
    public async Task 운송종료뒤에는_확정기사정보를유지하되_마지막위치를노출하지않는다()
    {
        await using var db = CreateContext();
        db.화주운송의뢰.Add(CreateRequest());
        db.운송원장.Add(new 운송원장
        {
            의뢰Id = "request-1",
            운송번호 = "transport-1",
            확정기사Id = "driver-1",
            기사_운송자 = "driver-1",
            상태 = 상태값.배차상태.인수완료,
            UpdatedAt = DateTime.UtcNow
        });
        db.용달기사.Add(new 용달기사 { 기사Id = "driver-1", 기사명 = "안전기사" });
        db.기사위치기록.Add(new 기사위치기록
        {
            기사Id = "driver-1",
            위도 = 37.501m,
            경도 = 127.039m,
            기록시각 = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new 의뢰단건조회QueryHandler(
            db,
            new TestCurrentUserAccessor("shipper-1", "화주"));

        var response = await handler.Handle(
            new 의뢰단건조회Query("request-1"),
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("안전기사", response!.확정기사명);
        Assert.Null(response.기사최근위도);
        Assert.Null(response.기사최근경도);
        Assert.Null(response.기사최근위치시각Utc);
    }

    private static 화주운송의뢰 CreateRequest()
        => new()
        {
            의뢰Id = "request-1",
            주문자UserId = "shipper-1",
            화주Id = "shipper-1",
            상태 = "생성됨",
            배차상태 = 상태값.배차상태.배차확정,
            결제상태 = 상태값.결제상태.결제완료,
            픽업_도로명주소 = "서울시 강남구",
            하차_도로명주소 = "서울시 송파구"
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"shipper-request-query-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
