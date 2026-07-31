using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Drivers;
using Ssalddel.Contracts.Common.Transport;
using Ssalddel.Infrastructure.Storage.Memory;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.DeliveryZones;
using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Engine;
using 살뜰.Services.Dispatch.Queue;
using 살뜰.도메인.공통;
using 살뜰.도메인.기사;
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
        var foodExecutionStore = new InMemory음식배달권실행공간Store();
        var cargoExecutionStore = new InMemory국내화물배달권실행공간Store();
        var projectionService = new 원장배달권투영Service(db);
        var transportBridge = new 운송원장배달권연결Service(projectionService);
        var service = new 배차실행인덱스예열Service(
            db,
            new InMemoryDriverWorkQueueStore(),
            new InMemoryDriverLocationStore(),
            new InMemory국내화물운송기사상태Store(),
            transportBridge,
            foodExecutionStore,
            cargoExecutionStore);

        var result = await service.예열Async();

        Assert.Equal(2, result.미처리운송의뢰수);
        Assert.Equal(5, await db.원장배달권투영.CountAsync());
        Assert.Single(
            db.원장배달권투영,
            x => x.원장유형코드 == Ssalddel.Contracts.Common.DeliveryZones.원장배달권원장유형코드.음식주문);
        var executionZones = await foodExecutionStore.SnapshotAsync();
        var executionZone = Assert.Single(executionZones);
        Assert.Equal(new[] { "FOOD-EXISTING" }, executionZone.미처리운송의뢰Ids);
        Assert.Empty(await cargoExecutionStore.SnapshotAsync());
    }

    [Fact]
    public async Task 서버예열은_근무에_저장된_실행유형대로_Food와_Cargo_물리공간을_복구한다()
    {
        await using var db = CreateContext();
        var now = DateTime.UtcNow;
        db.용달기사.AddRange(
            new 용달기사
            {
                기사Id = "FOOD-DRIVER-1",
                기사명 = "음식기사",
                상태 = "활동중",
                운행상태 = 상태값.기사운행상태.운행중,
                주_활동지역 = "서울특별시 중랑구",
                UpdatedAt = now
            },
            new 용달기사
            {
                기사Id = "CARGO-DRIVER-1",
                기사명 = "화물기사",
                상태 = "활동중",
                운행상태 = 상태값.기사운행상태.운행중,
                주_활동지역 = "서울특별시 중랑구",
                UpdatedAt = now
            });
        db.기사근무.AddRange(
            new 기사근무
            {
                기사Id = "FOOD-DRIVER-1",
                시작모드 = "immediate",
                시작시각 = now.AddMinutes(-30),
                시작위치 = "서울특별시 중랑구",
                운송실행유형 = 운송실행유형코드.음식배달,
                CreatedAt = now.AddMinutes(-30),
                UpdatedAt = now
            },
            new 기사근무
            {
                기사Id = "CARGO-DRIVER-1",
                시작모드 = "immediate",
                시작시각 = now.AddMinutes(-20),
                시작위치 = "서울특별시 중랑구",
                운송실행유형 = 운송실행유형코드.화물운송,
                CreatedAt = now.AddMinutes(-20),
                UpdatedAt = now
            });
        db.기사위치기록.AddRange(
            new 기사위치기록
            {
                기사Id = "FOOD-DRIVER-1",
                위도 = 37.6063m,
                경도 = 127.0927m,
                기록시각 = now.AddMinutes(-1),
                CreatedAt = now.AddMinutes(-1),
                UpdatedAt = now.AddMinutes(-1)
            },
            new 기사위치기록
            {
                기사Id = "CARGO-DRIVER-1",
                위도 = 37.6064m,
                경도 = 127.0928m,
                기록시각 = now.AddMinutes(-1),
                CreatedAt = now.AddMinutes(-1),
                UpdatedAt = now.AddMinutes(-1)
            });
        await db.SaveChangesAsync();
        var foodExecutionStore = new InMemory음식배달권실행공간Store();
        var cargoExecutionStore = new InMemory국내화물배달권실행공간Store();
        var driverStateStore = new InMemory국내화물운송기사상태Store();
        var service = new 배차실행인덱스예열Service(
            db,
            new InMemoryDriverWorkQueueStore(),
            new InMemoryDriverLocationStore(),
            driverStateStore,
            new 운송원장배달권연결Service(new 원장배달권투영Service(db)),
            foodExecutionStore,
            cargoExecutionStore);

        await service.예열Async();

        var foodSpace = Assert.Single(await foodExecutionStore.SnapshotAsync());
        var cargoSpace = Assert.Single(await cargoExecutionStore.SnapshotAsync());
        Assert.Equal(["FOOD-DRIVER-1"], foodSpace.운행중기사Ids);
        Assert.Equal(["CARGO-DRIVER-1"], cargoSpace.운행중기사Ids);
        Assert.Equal(
            기사앱식별자.FoodDeliveryDriverApp,
            (await driverStateStore.GetAsync("FOOD-DRIVER-1"))?.AppKey);
        Assert.Equal(
            기사앱식별자.CargoYongdalDriverApp,
            (await driverStateStore.GetAsync("CARGO-DRIVER-1"))?.AppKey);
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
