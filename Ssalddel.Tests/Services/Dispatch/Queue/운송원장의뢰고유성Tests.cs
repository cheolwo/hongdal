using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Ssalddel.Contracts.Common.DeliveryZones;
using Ssalddel.Contracts.Common.Warehouse;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Engine;
using 살뜰.Services.Dispatch.Queue;
using 살뜰.Services.DeliveryZones;
using 살뜰.도메인.운송;

namespace Ssalddel.Tests.Services.Dispatch.Queue;

public sealed class 운송원장의뢰고유성Tests
{
    [Fact]
    public void 운송원장은_의뢰Id에_고유인덱스를가진다()
    {
        using var db = new SsalddelContext(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseMySql(
                    "Server=localhost;Database=ssalddel_dispatch_unique_test;User=root;Password=test;",
                    new MySqlServerVersion(new Version(8, 4, 0)))
                .Options,
            new DummyPersonalDataEncryptionService());

        var entityType = Assert.IsAssignableFrom<Microsoft.EntityFrameworkCore.Metadata.IReadOnlyEntityType>(
            db.Model.FindEntityType(typeof(운송원장)));
        var requestIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Count == 1
                     && index.Properties[0].Name == nameof(운송원장.의뢰Id));

        Assert.True(requestIndex.IsUnique);
        Assert.Equal("ux_운송실행투영_request_id", requestIndex.GetDatabaseName());
    }

    [Fact]
    public async Task 같은_의뢰를_다시처리하면_기존원장을반환한다()
    {
        await using var db = new SsalddelContext(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"dispatch-ledger-idempotency-{Guid.NewGuid():N}")
                .Options,
            new DummyPersonalDataEncryptionService());
        var service = new 운송의뢰배차대기Service(
            db,
            new 운송의뢰배차원천분류Service(),
            new 운송원장배달권연결Service(new 원장배달권투영Service(db)),
            new InMemory배달권실행공간Store());
        var target = new 출고예정운송대상
        {
            원천유형 = 출고예정운송대상원천유형.화주운송의뢰,
            원천참조번호 = "REQUEST-UNIQUE-1",
            운송의뢰Id = "REQUEST-UNIQUE-1",
            판매자UserId = "SHIPPER-1",
            상차주소 = "서울시 중구",
            하차주소 = "서울시 강남구"
        };

        var first = await service.생성또는조회Async(target);
        await db.SaveChangesAsync();
        var second = await service.생성또는조회Async(target);

        Assert.Equal(first.Id, second.Id);
        Assert.Single(db.운송원장);
        Assert.Equal(2, await db.원장배달권투영.CountAsync());
    }

    [Theory]
    [InlineData(운송의뢰배차원천유형.음식주문, 원장배달권원장유형코드.음식주문, 원장배달권역할코드.배송)]
    [InlineData(운송의뢰배차원천유형.살뜰마트주문, 원장배달권원장유형코드.마트주문, 원장배달권역할코드.배송)]
    [InlineData(운송의뢰배차원천유형.공동주문국내운송, 원장배달권원장유형코드.같이주문, 원장배달권역할코드.배송)]
    [InlineData(운송의뢰배차원천유형.Fcl연계운송, 원장배달권원장유형코드.같이수입, 원장배달권역할코드.국내인계)]
    public async Task 운송원장은_네가지_원천원장을_플랫폼배달권에연결한다(
        string sourceType,
        string expectedLedgerType,
        string expectedRole)
    {
        await using var db = new SsalddelContext(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"dispatch-source-zone-{Guid.NewGuid():N}")
                .Options,
            new DummyPersonalDataEncryptionService());
        var service = new 운송의뢰배차대기Service(
            db,
            new 운송의뢰배차원천분류Service(),
            new 운송원장배달권연결Service(new 원장배달권투영Service(db)),
            new InMemory배달권실행공간Store());
        var requestId = $"REQUEST-{expectedLedgerType}";

        await service.생성또는조회Async(
            new 출고예정운송대상
            {
                원천유형 = 출고예정운송대상원천유형.화주운송의뢰,
                원천참조번호 = requestId,
                운송의뢰Id = requestId,
                판매자UserId = "SHIPPER-1",
                상차주소 = "서울특별시 중구 세종대로",
                하차주소 = "서울특별시 강남구 테헤란로"
            },
            new 운송의뢰배차대기생성옵션
            {
                원본의뢰유형 = sourceType,
                원본의뢰Id = $"SOURCE-{expectedLedgerType}"
            });
        await db.SaveChangesAsync();

        var sourceProjection = Assert.Single(
            db.원장배달권투영,
            x => x.원장유형코드 == expectedLedgerType);
        Assert.Equal($"SOURCE-{expectedLedgerType}", sourceProjection.원장Id);
        Assert.Equal(expectedRole, sourceProjection.역할코드);
        Assert.Equal(
            2,
            await db.원장배달권투영.CountAsync(
                x => x.원장유형코드 == 원장배달권원장유형코드.운송원장));
    }

    [Fact]
    public async Task 미정배달권_의뢰는_영속투영만남기고_기사실행공간에는넣지않는다()
    {
        await using var db = new SsalddelContext(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"dispatch-unknown-zone-{Guid.NewGuid():N}")
                .Options,
            new DummyPersonalDataEncryptionService());
        var executionStore = new InMemory배달권실행공간Store();
        var service = new 운송의뢰배차대기Service(
            db,
            new 운송의뢰배차원천분류Service(),
            new 운송원장배달권연결Service(new 원장배달권투영Service(db)),
            executionStore);

        await service.생성또는조회Async(new 출고예정운송대상
        {
            원천유형 = 출고예정운송대상원천유형.화주운송의뢰,
            원천참조번호 = "REQUEST-UNKNOWN-ZONE",
            운송의뢰Id = "REQUEST-UNKNOWN-ZONE",
            판매자UserId = "SHIPPER-1"
        });
        await db.SaveChangesAsync();

        Assert.Equal(2, await db.원장배달권투영.CountAsync());
        Assert.Empty(await executionStore.SnapshotAsync());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
