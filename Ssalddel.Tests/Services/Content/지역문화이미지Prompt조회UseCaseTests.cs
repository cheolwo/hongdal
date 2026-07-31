using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Controllers.Admin.Content07;
using Ssalddel.Domain.Content;
using Ssalddel.Infrastructure.Persistence.SeedData.Content;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Content;

public sealed class 지역문화이미지Prompt조회UseCaseTests
{
    [Fact]
    public async Task Seed는_한국17개시도와미국50개주와중국본토31개성급지역을저장한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        var useCase = new 지역문화이미지Prompt조회UseCase(db);

        var all = await useCase.목록조회Async(null);
        var korea = await useCase.목록조회Async(
            RegionalCultureImagePromptCountryCodes.Korea);
        var unitedStates = await useCase.목록조회Async(
            RegionalCultureImagePromptCountryCodes.UnitedStates);
        var china = await useCase.목록조회Async(
            RegionalCultureImagePromptCountryCodes.China);

        Assert.Equal(98, all.TotalCount);
        Assert.Equal(17, korea.TotalCount);
        Assert.Equal(50, unitedStates.TotalCount);
        Assert.Equal(31, china.TotalCount);
        Assert.All(all.Items, item =>
        {
            Assert.Equal("ResearchDraft", item.ReviewStatusCode);
            Assert.True(item.RequiresEvidenceReview);
            Assert.Equal("16:9", item.AspectRatio);
            Assert.Equal("center-4:3", item.SafeCrop);
            Assert.Equal(2, item.PromptVersion);
            Assert.Equal(
                RegionalCultureAnimationStyleCodes.CinematicStylized3D,
                item.VisualStyleCode);
            Assert.Equal(
                RegionalCultureAnimationStyleCodes.TargetImagesPerRegion,
                item.TargetImageCount);
            Assert.NotEmpty(item.VisualAnchors);
            Assert.NotEmpty(item.AvoidExpressions);
            Assert.Contains("스타일라이즈드 3D 애니메이션", item.PromptKo, StringComparison.Ordinal);
            Assert.Contains("생성 전", item.PromptKo, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task 서울Prompt는_생활문화와공식근거재검토경계를함께반환한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        var useCase = new 지역문화이미지Prompt조회UseCase(db);

        var item = await useCase.상세조회Async("KR-SEOUL");

        Assert.NotNull(item);
        Assert.Equal("kr-seoul", item!.RegionKey);
        Assert.Equal("KR-11", item.SubdivisionCode);
        Assert.Equal(
            지역문화행정구역유형Codes.KoreaSpecialCity,
            item.RegionTypeCode);
        Assert.Contains(item.VisualAnchors, anchor => anchor.Contains("동네 시장", StringComparison.Ordinal));
        Assert.Contains("궁궐·한복", item.PromptKo, StringComparison.Ordinal);
        Assert.Contains("국가유산청 국가유산포털", item.EvidenceNotesKo, StringComparison.Ordinal);
        Assert.Contains("지역문화진흥원", item.EvidenceNotesKo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 산둥Prompt는_지형생활문화안전경계를함께반환한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        var useCase = new 지역문화이미지Prompt조회UseCase(db);

        var item = await useCase.상세조회Async("CN-SHANDONG");

        Assert.NotNull(item);
        Assert.Equal("cn-shandong", item!.RegionKey);
        Assert.Equal("CN-37", item.SubdivisionCode);
        Assert.Contains(item.VisualAnchors, anchor => anchor.Contains("웨이팡 연", StringComparison.Ordinal));
        Assert.Contains("서양식 등대마을", item.PromptKo, StringComparison.Ordinal);
        Assert.Contains("중국 비물질문화유산망", item.EvidenceNotesKo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 사람이검토한Prompt는_초기Seed가덮어쓰지않는다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        var maine = await db.지역문화이미지Prompts.SingleAsync(
            item => item.RegionKey == "us-maine");
        maine.ReviewStatusCode = 지역문화이미지Prompt검토상태Codes.EvidenceReviewed;
        maine.PromptVersion = 0;
        maine.PromptKo = "사람이 근거를 검토한 프롬프트";
        await db.SaveChangesAsync();

        var changed = await 지역문화이미지PromptSeeder.SeedAsync(db);
        var reloaded = await db.지역문화이미지Prompts
            .AsNoTracking()
            .SingleAsync(item => item.RegionKey == "us-maine");

        Assert.Equal(0, changed);
        Assert.Equal("사람이 근거를 검토한 프롬프트", reloaded.PromptKo);
        Assert.Equal(지역문화이미지Prompt검토상태Codes.EvidenceReviewed, reloaded.ReviewStatusCode);
    }

    [Fact]
    public async Task 지원하지않는국가Filter는_명시적으로거절한다()
    {
        await using var db = CreateContext();
        var useCase = new 지역문화이미지Prompt조회UseCase(db);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.목록조회Async("JP"));

        Assert.Contains("KR, US, CN", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Controller는_관리자조회Route와무효과경계를유지한다()
    {
        var controller = typeof(지역문화이미지PromptController);
        var route = Assert.Single(controller.GetCustomAttributes(
            typeof(RouteAttribute),
            inherit: false).Cast<RouteAttribute>());
        var authorize = Assert.Single(controller.GetCustomAttributes(
            typeof(AuthorizeAttribute),
            inherit: false).Cast<AuthorizeAttribute>());

        Assert.Equal(
            "api/v1/admin/content/information/regional-culture/image-prompts",
            route.Template);
        Assert.Equal("서버관리자전용", authorize.Policy);
        Assert.NotNull(controller.GetMethod(nameof(지역문화이미지PromptController.목록조회)));
        Assert.NotNull(controller.GetMethod(nameof(지역문화이미지PromptController.상세조회)));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
