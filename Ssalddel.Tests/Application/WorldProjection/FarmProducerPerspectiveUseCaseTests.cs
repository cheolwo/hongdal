using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Common.WorldProjection;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.농업;

namespace Ssalddel.Tests.Application.WorldProjection;

public sealed class FarmProducerPerspectiveUseCaseTests
{
    [Fact]
    public async Task 생산자관점은_본인농장만_작물기준과운영상태를분리해_투영한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var useCase = new FarmProducerPerspectiveUseCase(db, new CurrentUser("producer-a"));

        var result = await useCase.QueryAsync();

        Assert.True(result.IsSuccess);
        var farm = Assert.Single(result.Value.Farms);
        Assert.Equal("farm:a", farm.StableId);
        var plot = Assert.Single(farm.Plots);
        var cultivation = Assert.Single(plot.Cultivations);
        Assert.Equal("감자", cultivation.CropName);
        Assert.Equal("crop-reference-category:fc01", cultivation.CropReferenceStableId);
        Assert.Equal("nongsaro:crop-ebook", cultivation.CropReferenceSourceKey);
        Assert.Equal(재배생육상태코드.생육중, cultivation.GrowthStatusCode);
    }

    [Fact]
    public async Task 센서관점은_최신관측과서버판정근거를_그대로제공한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var useCase = new FarmProducerPerspectiveUseCase(db, new CurrentUser("producer-a"));

        var result = await useCase.QueryAsync();

        var observation = Assert.Single(Assert.Single(Assert.Single(result.Value.Farms).Plots).Sensors)
            .LatestObservation;
        Assert.NotNull(observation);
        Assert.Equal(18.5m, observation!.Value);
        Assert.Equal(FarmSensorConditionCodes.Dry, observation.ConditionCode);
        Assert.Equal("SOIL-WATER-001", observation.EvidenceCardId);
        Assert.Equal("soil-water-rule:3", observation.AssessmentRuleRevision);
    }

    [Fact]
    public async Task 인증정보가없으면_운영데이터를반환하지않는다()
    {
        await using var db = CreateContext();
        var result = await new FarmProducerPerspectiveUseCase(db, new CurrentUser(null)).QueryAsync();

        Assert.True(result.IsFailed);
        Assert.Equal(401, result.Errors[0].Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 생산자Npc는_canonical농장작업과semanticWaypoint만_투영한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var result = await new FarmProducerPerspectiveUseCase(db, new CurrentUser("producer-a"))
            .QueryAsync();

        var worker = Assert.Single(result.Value.Workers);
        Assert.Equal("farm-worker:a.1", worker.NpcStableId);
        Assert.Equal("farm-task:a.inspect.1", worker.CanonicalTaskStableId);
        Assert.Equal("farm.field-a", worker.CurrentWaypointKey);
        Assert.Equal("farm.sensor-a", worker.DestinationWaypointKey);
        Assert.Equal(NpcMovementSourceTypeCodes.OperationalProjection, worker.SourceTypeCode);
    }

    [Fact]
    public void Unity농장Contract에는_소유자와위치개인정보필드가없다()
    {
        var names = typeof(FarmResponse).GetProperties().Select(property => property.Name)
            .Concat(typeof(FarmPlotResponse).GetProperties().Select(property => property.Name))
            .ToArray();

        Assert.DoesNotContain("OwnerUserId", names);
        Assert.DoesNotContain("소유자UserId", names);
        Assert.DoesNotContain("Address", names);
        Assert.DoesNotContain("Latitude", names);
        Assert.DoesNotContain("Longitude", names);
    }

    private static async Task SeedAsync(SsalddelContext db)
    {
        var owned = new 농장 { StableId = "farm:a", 소유자UserId = "producer-a", 농장명 = "A 농장", Revision = 4 };
        var other = new 농장 { StableId = "farm:b", 소유자UserId = "producer-b", 농장명 = "B 농장", Revision = 8 };
        db.농장.AddRange(owned, other);
        await db.SaveChangesAsync();
        var plot = new 농장구획 { 농장Id = owned.Id, StableId = "farm-plot:a.1", 구획명 = "1번 밭", Revision = 5 };
        db.농장구획.Add(plot);
        await db.SaveChangesAsync();
        db.재배작기.Add(new 재배작기
        {
            농장구획Id = plot.Id,
            StableId = "cultivation:a.potato.2026",
            작물명 = "감자",
            작물기준StableId = "crop-reference-category:fc01",
            작물기준SourceKey = "nongsaro:crop-ebook",
            생육상태Code = 재배생육상태코드.생육중,
            Revision = 6,
        });
        var sensor = new 농업센서
        {
            농장구획Id = plot.Id,
            StableId = "sensor:a.soil-moisture.1",
            센서유형Code = "SoilMoisture",
            Revision = 7,
        };
        db.농업센서.Add(sensor);
        await db.SaveChangesAsync();
        db.농업센서관측.AddRange(
            new 농업센서관측
            {
                농업센서Id = sensor.Id,
                관측값 = 24m,
                단위Code = "Percent",
                관측시각Utc = new DateTime(2026, 8, 8, 0, 0, 0, DateTimeKind.Utc),
                판정상태Code = 센서관측판정코드.정상,
                판정규칙Revision = "soil-water-rule:2",
            },
            new 농업센서관측
            {
                농업센서Id = sensor.Id,
                관측값 = 18.5m,
                단위Code = "Percent",
                관측시각Utc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
                판정상태Code = 센서관측판정코드.건조,
                판정규칙Revision = "soil-water-rule:3",
                근거카드Id = "SOIL-WATER-001",
                확신도Code = "Medium",
                판정한계 = "토성과 생육 단계에 따라 해석 범위가 달라집니다.",
            });
        db.농장작업.Add(new 농장작업
        {
            농장Id = owned.Id,
            농장구획Id = plot.Id,
            StableId = "farm-task:a.inspect.1",
            NpcStableId = "farm-worker:a.1",
            작업유형Code = "InspectSensor",
            RouteCode = "farm-producer-round",
            CurrentWaypointKey = "farm.field-a",
            DestinationWaypointKey = "farm.sensor-a",
            MovementStateCode = "Moving",
            ArrivalActionCode = "InspectSensor",
            Revision = 8,
            UpdatedAtUtc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
        });
        await db.SaveChangesAsync();
    }

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"farm-producer-{Guid.NewGuid():N}").Options,
            new DummyEncryption());

    private sealed class CurrentUser(string? userId) : ICurrentUserAccessor
    {
        public string? UserId => userId;
        public string? Role => null;
    }

    private sealed class DummyEncryption : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
