using Microsoft.EntityFrameworkCore;
using Ssalddel.Infrastructure.Storage.Memory;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.DeliveryZones;
using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Engine;
using 살뜰.Services.Dispatch.Queue;
using 살뜰.도메인.공통;
using 살뜰.도메인.운송;

namespace Ssalddel.Tests.Services.Dispatch.Queue;

public sealed class 배차실행인덱스예열ServiceTests
{
    [Fact]
    public async Task 기존_미처리운송원장을_플랫폼배달권으로_보강하고_미정권은_실행공간에서제외한다()
    {
        await using var db = CreateContext();
        db.운송원장.AddRange(
            CreateQueue(
                "FOOD-EXISTING",
                운송의뢰배차원천유형.음식주문,
                "FOOD-SOURCE-1",
                "서울특별시 중구 세종대로",
                "서울특별시 강남구 테헤란로"),
            CreateQueue(
                "UNKNOWN-EXISTING",
                운송의뢰배차원천유형.화주운송의뢰,
                "CARGO-SOURCE-1",
                string.Empty,
                string.Empty));
        await db.SaveChangesAsync();
        var executionStore = new InMemory배달권실행공간Store();
        var projectionService = new 원장배달권투영Service(db);
        var transportBridge = new 운송원장배달권연결Service(projectionService);
        var service = new 배차실행인덱스예열Service(
            db,
            new InMemoryDriverWorkQueueStore(),
            new InMemoryDriverLocationStore(),
            new InMemory국내화물운송기사상태Store(),
            transportBridge,
            executionStore);

        var result = await service.예열Async();

        Assert.Equal(2, result.미처리운송의뢰수);
        Assert.Equal(5, await db.원장배달권투영.CountAsync());
        Assert.Single(
            db.원장배달권투영,
            x => x.원장유형코드 == Ssalddel.Contracts.Common.DeliveryZones.원장배달권원장유형코드.음식주문);
        var executionZones = await executionStore.SnapshotAsync();
        var executionZone = Assert.Single(executionZones);
        Assert.Equal(new[] { "FOOD-EXISTING" }, executionZone.미처리운송의뢰Ids);
    }

    private static 운송원장 CreateQueue(
        string requestId,
        string sourceType,
        string sourceId,
        string pickupAddress,
        string dropoffAddress)
        => new()
        {
            운송번호 = requestId,
            의뢰Id = requestId,
            화주Id = "SHIPPER-1",
            원본의뢰유형 = sourceType,
            원본의뢰Id = sourceId,
            픽업_도로명주소 = pickupAddress,
            하차_도로명주소 = dropoffAddress,
            상태 = 상태값.배차대기상태.대기,
            배차업무유형 = sourceType == 운송의뢰배차원천유형.음식주문
                ? 상태값.배차업무유형.음식배달
                : 상태값.배차업무유형.용달운송,
            배차큐단계 = 상태값.배차큐단계.계획배차,
            배차노출상태 = 상태값.배차노출상태.계획대기
        };

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"dispatch-index-warmup-{Guid.NewGuid():N}")
                .Options,
            new DummyPersonalDataEncryptionService());

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
