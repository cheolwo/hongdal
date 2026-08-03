using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Infrastructure.Persistence.SeedData.Content;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Content;

public sealed class 지역문화공공기관Source조회UseCaseTests
{
    [Fact]
    public async Task Seed는_한국미국중국의공식기관원천을저장한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();

        var changed = await 지역문화공공기관SourceSeeder.SeedAsync(db);
        var useCase = new 지역문화공공기관Source조회UseCase(db);
        var all = await useCase.목록조회Async(null, null);
        var korea = await useCase.목록조회Async("kr", null);
        var unitedStates = await useCase.목록조회Async("US", null);
        var china = await useCase.목록조회Async("cn", null);

        Assert.Equal(16, changed);
        Assert.Equal(16, all.TotalCount);
        Assert.Equal(6, korea.TotalCount);
        Assert.Equal(6, unitedStates.TotalCount);
        Assert.Equal(4, china.TotalCount);
        Assert.All(all.Items, item =>
        {
            Assert.StartsWith("https://", item.OfficialPageUrl, StringComparison.Ordinal);
            Assert.StartsWith("https://", item.DataUrl, StringComparison.Ordinal);
            Assert.True(item.RequiresRegionalVerification);
            Assert.NotEmpty(item.LimitationsKo);
            Assert.Equal(
                item.CountryCode == RegionalCulturePublicInstitutionCountryCodes.China
                    ? new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc)
                    : new DateTime(2026, 7, 26, 0, 0, 0, DateTimeKind.Utc),
                item.EvidenceCheckedAtUtc);
        });
    }

    [Fact]
    public async Task 관할단계Filter는_행정복지센터와미국주기관을구분한다()
    {
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화공공기관SourceSeeder.SeedAsync(db);
        var useCase = new 지역문화공공기관Source조회UseCase(db);

        var koreanNeighborhood = await useCase.목록조회Async(
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.Neighborhood);
        var usStates = await useCase.목록조회Async(
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince);

        var neighborhood = Assert.Single(koreanNeighborhood.Items);
        Assert.Equal("kr-mois-administrative-agency-jurisdiction", neighborhood.SourceKey);
        Assert.Contains("주민센터", neighborhood.LimitationsKo, StringComparison.Ordinal);
        Assert.Equal(2, usStates.TotalCount);
        Assert.Contains(usStates.Items, item =>
            item.SourceKey == "us-nea-state-regional-arts-organizations");
        Assert.Contains(usStates.Items, item =>
            item.SourceKey == "us-nps-state-historic-preservation-offices");
    }

    [Fact]
    public async Task 지원하지않는국가와관할단계는_명시적으로거절한다()
    {
        await using var db = CreateContext();
        var useCase = new 지역문화공공기관Source조회UseCase(db);

        var countryException = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.목록조회Async("JP", null));
        var levelException = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.목록조회Async("KR", "VillageOffice"));

        Assert.Contains("KR, US, CN", countryException.Message, StringComparison.Ordinal);
        Assert.Contains("JurisdictionLevelCode", levelException.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void 공개Controller와Metadata는_읽기전용경계를표현한다()
    {
        var controllerType = typeof(지역문화공공기관Controller);
        var route = Assert.Single(controllerType.GetCustomAttributes(
            typeof(RouteAttribute),
            inherit: false).Cast<RouteAttribute>());
        Assert.Single(controllerType.GetCustomAttributes(
            typeof(AllowAnonymousAttribute),
            inherit: false));

        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.RegionalCulturePublicInstitution,
            typeof(RegionalCulturePublicInstitutionSourceDto).Assembly,
            typeof(지역문화공공기관Source조회UseCase).Assembly);

        Assert.Equal(
            "api/v1/community/regional-culture/public-institutions",
            route.Template);
        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(RegionalCulturePublicInstitutionSourceDto));
        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(지역문화공공기관Source조회UseCase)
            && item.Effects == SsalddelCodeEffect.PersistentRead);
        Assert.Contains(metadata, item =>
            item.ComponentType == controllerType
            && item.Effects == SsalddelCodeEffect.PersistentRead);
        Assert.DoesNotContain(metadata, item =>
            item.Effects.HasFlag(SsalddelCodeEffect.PersistentWrite)
            || item.Effects.HasFlag(SsalddelCodeEffect.NetworkCall));
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
