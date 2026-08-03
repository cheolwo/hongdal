using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Infrastructure.Persistence.SeedData.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Content;

public sealed class RegionalCultureImageResearchReadinessTests
{
    [Fact]
    public async Task 준비Manifest는_98개지역과등록된공식원천을포함하고_생성을승인하지않는다()
    {
        var manifest = await ReadManifestAsync();
        Assert.Equal(1, manifest.SchemaVersion);
        Assert.False(manifest.GenerationAuthorized);
        Assert.Equal("ResearchDraft", manifest.DefaultReviewStatus);
        Assert.Equal(10, manifest.TargetImagesPerRegion);
        Assert.Equal(98, manifest.RegionTotal);
        Assert.Equal(3, manifest.Countries.Count);
        Assert.All(manifest.Countries, country =>
        {
            Assert.Equal(country.ExpectedRegionCount, country.RegionKeys.Count);
            Assert.True(country.OfficialSourceKeys.Count >= 2);
        });

        var manifestRegionKeys = manifest.Countries
            .SelectMany(country => country.RegionKeys)
            .ToHashSet(StringComparer.Ordinal);
        var manifestSourceKeysByCountry = manifest.Countries.ToDictionary(
            country => country.CountryCode,
            country => country.OfficialSourceKeys.ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);

        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await 지역문화이미지PromptSeeder.SeedAsync(db);
        await 지역문화공공기관SourceSeeder.SeedAsync(db);

        var promptRegionKeys = await db.지역문화이미지Prompts
            .AsNoTracking()
            .Select(prompt => prompt.RegionKey)
            .ToArrayAsync();
        Assert.True(
            promptRegionKeys.ToHashSet(StringComparer.Ordinal).SetEquals(manifestRegionKeys));

        var sources = await db.지역문화공공기관Sources
            .AsNoTracking()
            .Select(source => new { source.CountryCode, source.SourceKey })
            .ToArrayAsync();
        foreach (var country in manifestSourceKeysByCountry)
        {
            var registered = sources
                .Where(source => source.CountryCode == country.Key)
                .Select(source => source.SourceKey)
                .ToHashSet(StringComparer.Ordinal);
            Assert.True(country.Value.IsSubsetOf(registered));
        }

        Assert.All(
            await db.지역문화이미지Prompts.AsNoTracking().ToArrayAsync(),
            prompt =>
            {
                Assert.Equal("ResearchDraft", prompt.ReviewStatusCode);
                Assert.True(prompt.RequiresEvidenceReview);
            });
    }

    private static async Task<ResearchReadinessManifest> ReadManifestAsync()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "docs",
            "Content",
            "RegionalCultureImagePrompts",
            "research-readiness.v1.json");
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ResearchReadinessManifest>(
                   json,
                   new JsonSerializerOptions(JsonSerializerDefaults.Web))
               ?? throw new InvalidOperationException("지역문화 이미지 조사 준비 manifest를 읽을 수 없습니다.");
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }

    private sealed record ResearchReadinessManifest(
        int SchemaVersion,
        bool GenerationAuthorized,
        string DefaultReviewStatus,
        int TargetImagesPerRegion,
        int RegionTotal,
        IReadOnlyList<ResearchReadinessCountry> Countries);

    private sealed record ResearchReadinessCountry(
        string CountryCode,
        int ExpectedRegionCount,
        IReadOnlyList<string> OfficialSourceKeys,
        IReadOnlyList<string> RegionKeys);

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
