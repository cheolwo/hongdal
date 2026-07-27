using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.DeliveryZones;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.DeliveryZones;

namespace Ssalddel.Tests.Services.DeliveryZones;

public sealed class 원장배달권투영ServiceTests
{
    [Fact]
    public async Task 네가지_주문원장이_같은_플랫폼배달권에_연결된다()
    {
        await using var db = CreateContext();
        var service = new 원장배달권투영Service(db);
        var ledgerTypes = new[]
        {
            원장배달권원장유형코드.음식주문,
            원장배달권원장유형코드.마트주문,
            원장배달권원장유형코드.같이주문,
            원장배달권원장유형코드.같이수입
        };

        foreach (var ledgerType in ledgerTypes)
        {
            await service.연결추적Async(new 원장배달권연결요청
            {
                원장유형코드 = ledgerType,
                원장Id = $"{ledgerType}-1",
                역할코드 = ledgerType == 원장배달권원장유형코드.같이수입
                    ? 원장배달권역할코드.국내인계
                    : 원장배달권역할코드.배송,
                도로명주소 = "서울특별시 강남구 테헤란로",
                생성근거 = "테스트 원장 연결"
            });
        }

        await db.SaveChangesAsync();

        Assert.Single(db.플랫폼배달권);
        Assert.Equal(4, await db.원장배달권투영.CountAsync());
        Assert.Equal(
            ledgerTypes.OrderBy(x => x, StringComparer.Ordinal),
            await db.원장배달권투영
                .Select(x => x.원장유형코드)
                .OrderBy(x => x)
                .ToArrayAsync());
    }

    [Fact]
    public async Task 같은_원장과_역할은_새_배달권으로_멱등갱신된다()
    {
        await using var db = CreateContext();
        var service = new 원장배달권투영Service(db);
        var request = new 원장배달권연결요청
        {
            원장유형코드 = 원장배달권원장유형코드.음식주문,
            원장Id = "FOOD-100",
            역할코드 = 원장배달권역할코드.배송,
            도로명주소 = "서울특별시 강남구 테헤란로",
            생성근거 = "음식 주문 배송지"
        };

        var first = await service.연결추적Async(request);
        await db.SaveChangesAsync();
        request.도로명주소 = "서울특별시 중구 세종대로";
        var second = await service.연결추적Async(request);
        await db.SaveChangesAsync();

        Assert.NotEqual(first.배달권.배달권키, second.배달권.배달권키);
        Assert.Single(db.원장배달권투영);
        var result = Assert.Single(await service.조회Async(
            원장배달권원장유형코드.음식주문,
            "FOOD-100"));
        Assert.Equal(second.배달권.배달권키, result.배달권.배달권키);
    }

    [Fact]
    public async Task 주소와_좌표가_없으면_미정배달권으로_보존하되_활성화하지않는다()
    {
        await using var db = CreateContext();
        var service = new 원장배달권투영Service(db);

        var result = await service.연결추적Async(new 원장배달권연결요청
        {
            원장유형코드 = 원장배달권원장유형코드.같이주문,
            원장Id = "TOGETHER-UNKNOWN",
            역할코드 = 원장배달권역할코드.집결,
            생성근거 = "집결지 미정"
        });
        await db.SaveChangesAsync();

        Assert.Equal("unknown", result.배달권.배달권키);
        Assert.False((await db.플랫폼배달권.SingleAsync()).활성);
    }

    [Fact]
    public async Task 기존_주문자집단배송권키를_새_키로_바꾸지않고_연결한다()
    {
        await using var db = CreateContext();
        var service = new 원장배달권투영Service(db);

        var result = await service.연결추적Async(new 원장배달권연결요청
        {
            원장유형코드 = 원장배달권원장유형코드.같이주문,
            원장Id = "TOGETHER-EXISTING-SCOPE",
            역할코드 = 원장배달권역할코드.집결,
            기존배송권키 = "kr-admin2:11-680",
            기존배송권명 = "서울특별시 강남구 주문자 집단권",
            기존배송권판정방식 = "OperatingMarketDeliveryScope",
            생성근거 = "기존 같이 주문 배송권"
        });
        await db.SaveChangesAsync();
        var preserved = await service.연결추적Async(new 원장배달권연결요청
        {
            원장유형코드 = 원장배달권원장유형코드.같이주문,
            원장Id = "TOGETHER-EXISTING-SCOPE",
            역할코드 = 원장배달권역할코드.집결,
            도로명주소 = "서울특별시 중구 세종대로",
            생성근거 = "후속 운송 주소 판정",
            기존연결우선여부 = true
        });

        Assert.Equal("kr-admin2:11-680", result.배달권.배달권키);
        Assert.Equal("서울특별시 강남구 주문자 집단권", result.배달권.배달권명);
        Assert.Equal(result.배달권.배달권키, preserved.배달권.배달권키);
        Assert.True((await db.플랫폼배달권.SingleAsync()).활성);
    }

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"platform-delivery-zone-{Guid.NewGuid():N}")
                .Options,
            new DummyPersonalDataEncryptionService());

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
