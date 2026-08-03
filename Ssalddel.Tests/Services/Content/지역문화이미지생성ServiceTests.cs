using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Controllers.Admin.Content07;
using Ssalddel.Infrastructure.Persistence.SeedData.Content;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Images;
using 살뜰.Services.Options;
using 살뜰.도메인.공통;

namespace Ssalddel.Tests.Services.Content;

public sealed class 지역문화이미지생성ServiceTests
{
    [Fact]
    public void 애니메이션Prompt는_입체애니메이션과문화안전경계를고정한다()
    {
        var generator = new 지역문화애니메이션프롬프트생성기();

        var prompt = generator.CreatePrompt(new 이미지생성요청
        {
            이미지용도 = 생성이미지용도.지역문화애니메이션,
            대상타입 = 지역문화이미지대상Resolver.대상타입값,
            대상식별자 = "kr-seoul--scene-01",
            제목 = "서울특별시 장면 01",
            설명 = "서울의 한강과 산 능선, 동네 시장과 골목 공방, 한옥과 현대 주거의 공존",
            추가맥락 = "이른 아침 동네의 하루가 시작되는 장면"
        });

        Assert.Contains("stylized 3D animation", prompt, StringComparison.Ordinal);
        Assert.Contains("animation rather than", prompt, StringComparison.Ordinal);
        Assert.Contains("not documentary evidence", prompt, StringComparison.Ordinal);
        Assert.Contains("Do not imitate a named studio", prompt, StringComparison.Ordinal);
        Assert.Contains("No text", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 대상Resolver는_승인된한지역의누락장면을번호순으로반환한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        var seoul = await db.지역문화이미지Prompts
            .SingleAsync(item => item.RegionKey == "kr-seoul");
        seoul.ReviewStatusCode = Ssalddel.Domain.Content.지역문화이미지Prompt검토상태Codes.ApprovedForGeneration;
        seoul.RequiresEvidenceReview = false;
        await db.SaveChangesAsync();
        var resolver = CreateResolver(db);

        var targets = await resolver.GetMissingImageTargetsAsync(
            maxCount: 3,
            includeFailed: false);

        Assert.Equal(3, targets.Count);
        Assert.Equal(
            ["kr-seoul--scene-01", "kr-seoul--scene-02", "kr-seoul--scene-03"],
            targets.Select(item => item.대상식별자));
        Assert.All(targets, item =>
        {
            Assert.Equal("16:9", item.종횡비);
            Assert.Equal("1K", item.해상도);
            Assert.Equal(생성이미지용도.지역문화애니메이션, item.이미지용도);
            Assert.Contains("/10", item.추가맥락, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task 대상Resolver는_완료와진행중장면을건너뛰고실패는명시할때만재시도한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        var seoul = await db.지역문화이미지Prompts
            .SingleAsync(item => item.RegionKey == "kr-seoul");
        seoul.ReviewStatusCode = Ssalddel.Domain.Content.지역문화이미지Prompt검토상태Codes.ApprovedForGeneration;
        seoul.RequiresEvidenceReview = false;
        db.생성이미지작업.AddRange(
            CreateJob("kr-seoul--scene-01", 생성이미지작업상태.완료),
            CreateJob("kr-seoul--scene-02", 생성이미지작업상태.생성중),
            CreateJob("kr-seoul--scene-03", 생성이미지작업상태.실패));
        await db.SaveChangesAsync();
        var resolver = CreateResolver(db);

        var normal = await resolver.GetMissingImageTargetsAsync(
            maxCount: 10,
            includeFailed: false);
        var retry = await resolver.GetMissingImageTargetsAsync(
            maxCount: 1,
            includeFailed: true);

        Assert.Equal("kr-seoul--scene-04", normal[0].대상식별자);
        Assert.DoesNotContain(normal, item => item.대상식별자 == "kr-seoul--scene-03");
        Assert.Equal("kr-seoul--scene-03", Assert.Single(retry).대상식별자);
    }

    [Fact]
    public async Task 생성승인은_공식근거와고정관념검토를요구하고진행현황은10개슬롯을반환한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        await 지역문화공공기관SourceSeeder.SeedAsync(db);
        var useCase = new 지역문화이미지생성관리UseCase(
            db,
            new StubSequenceService(),
            Options.Create(CreateOptions()));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.생성승인Async(
                "kr-seoul",
                new RegionalCultureImageGenerationApprovalRequest
                {
                    OfficialSourcesReviewed = true,
                    StereotypeRiskReviewed = false,
                    ReviewedSourceKeys =
                    [
                        "kr-mcst-regional-culture-policy",
                        "kr-regional-culture-promotion-agency"
                    ],
                    ReviewNoteKo = "공식 기관 근거를 확인했지만 고정관념 검토는 아직 하지 않았습니다."
                }));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.생성승인Async(
                "kr-seoul",
                new RegionalCultureImageGenerationApprovalRequest
                {
                    OfficialSourcesReviewed = true,
                    StereotypeRiskReviewed = true,
                    ReviewedSourceKeys =
                    [
                        "us-nea-state-regional-arts-organizations",
                        "us-nps-state-historic-preservation-offices"
                    ],
                    ReviewNoteKo = "다른 국가의 원천으로 서울 지역 이미지를 승인하지 못하도록 확인하는 검토 메모입니다."
                }));

        var approval = await useCase.생성승인Async(
            "kr-seoul",
            new RegionalCultureImageGenerationApprovalRequest
            {
                OfficialSourcesReviewed = true,
                StereotypeRiskReviewed = true,
                ReviewedSourceKeys =
                [
                    "kr-mcst-regional-culture-policy",
                    "kr-regional-culture-promotion-agency"
                ],
                ReviewNoteKo = "서울의 공식 문화기관 자료와 생활문화 표현을 확인하고 단일 관광 상징으로 고정하지 않았습니다."
            });
        db.생성이미지작업.Add(CreateJob(
            "kr-seoul--scene-01",
            생성이미지작업상태.완료,
            "https://example.invalid/kr-seoul-01.png"));
        await db.SaveChangesAsync();

        var progress = await useCase.진행현황조회Async(
            RegionalCultureImagePromptCountryCodes.Korea);
        var seoul = Assert.Single(progress.Items, item => item.RegionKey == "kr-seoul");

        Assert.Equal(
            Ssalddel.Domain.Content.지역문화이미지Prompt검토상태Codes.ApprovedForGeneration,
            approval.ReviewStatusCode);
        Assert.False(approval.RequiresEvidenceReview);
        Assert.True(seoul.ReadyForGeneration);
        Assert.Equal(10, seoul.TargetCount);
        Assert.Equal(1, seoul.CompletedCount);
        Assert.Equal(9, seoul.RemainingCount);
        Assert.Equal(10, seoul.Slots.Count);
        Assert.Equal(
            RegionalCultureAnimationStyleCodes.CinematicStylized3D,
            progress.VisualStyleCode);
    }

    [Fact]
    public void 생성Controller는_서버관리자전용승인현황다음장면Route를제공한다()
    {
        var controller = typeof(지역문화이미지생성Controller);
        var route = Assert.Single(controller.GetCustomAttributes(
            typeof(RouteAttribute),
            inherit: false).Cast<RouteAttribute>());
        var authorize = Assert.Single(controller.GetCustomAttributes(
            typeof(AuthorizeAttribute),
            inherit: false).Cast<AuthorizeAttribute>());

        Assert.Equal(
            "api/v1/admin/content/information/regional-culture/image-generation",
            route.Template);
        Assert.Equal("서버관리자전용", authorize.Policy);
        Assert.NotNull(controller.GetMethod(nameof(지역문화이미지생성Controller.진행현황)));
        Assert.NotNull(controller.GetMethod(nameof(지역문화이미지생성Controller.생성승인)));
        Assert.NotNull(controller.GetMethod(nameof(지역문화이미지생성Controller.다음장면생성)));
    }

    private static 지역문화이미지대상Resolver CreateResolver(SsalddelContext db)
        => new(db, Options.Create(CreateOptions()));

    private static RegionalCultureImageGenerationOptions CreateOptions()
        => new()
        {
            TargetImagesPerRegion = 10,
            MaxNewJobsPerCycle = 1,
            MaxDailySubmissions = 10,
            CountryOrder = "KR,US,CN",
            AspectRatio = "16:9",
            Resolution = "1K"
        };

    private static 생성이미지작업 CreateJob(
        string targetIdentifier,
        string status,
        string? imageUrl = null)
        => new()
        {
            대상타입 = 지역문화이미지대상Resolver.대상타입값,
            대상식별자 = targetIdentifier,
            이미지용도 = 생성이미지용도.지역문화애니메이션,
            중복방지키 =
                $"{지역문화이미지대상Resolver.대상타입값}::{targetIdentifier}::{생성이미지용도.지역문화애니메이션}",
            프롬프트 = "test",
            상태 = status,
            저장Url = imageUrl,
            완료시각 = status == 생성이미지작업상태.완료
                ? DateTime.UtcNow
                : null,
            최종실패시각 = status == 생성이미지작업상태.실패
                ? DateTime.UtcNow
                : null
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class StubSequenceService : I지역문화이미지순차생성Service
    {
        public Task<지역문화이미지순차생성결과> 다음배치생성Async(
            int requestedCount,
            bool includeFailed,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 지역문화이미지순차생성결과(
                false,
                "Test",
                "테스트 대체 서비스",
                []));
    }

    private sealed class DummyPersonalDataEncryptionService
        : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
