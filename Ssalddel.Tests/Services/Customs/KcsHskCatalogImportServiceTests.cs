using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.HsCodes;
using Ssalddel.Services.Customs;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Customs;

public sealed class KcsHskCatalogImportServiceTests
{
    [Fact]
    public void Parser_preserves_official_chapter_heading_and_ten_digit_names()
    {
        const string chapterHtml = """
            <input type="hidden" name="sctYear" value="20260101" />
            <input type="hidden" name="hstdYear" value="20260101" />
            <table><tr>
              <th><a>10<input type="hidden" name="hstdCd" value="10" /></a></th>
              <td class="textLeft no_line_r"><a>제10류 곡물<input type="hidden" name="hstdCd" value="10" /></a></td>
            </tr></table>
            """;
        const string headingHtml = """
            <table><tr>
              <td><a>1006</a></td><td><a>쌀</a></td><td><a>Rice.</a></td>
              <td><input type="hidden" name="hsfdCd_Mn" value="1006" /></td>
            </tr></table>
            """;
        const string detailHtml = """
            <table>
              <tr><td><input name="hsSgn_Mn" value="100620" /><a>1006</a></td><td><a>20</a></td><td><a>　</a></td><td><a>현미</a></td><td><a>Husked (brown) rice</a></td></tr>
              <tr><td><input name="hsSgn_Mn" value="1006201000" /><a>1006</a></td><td><a>20</a></td><td><a>1000</a></td><td><a>메현미</a></td><td><a>Nonglutinous</a></td></tr>
            </table>
            """;

        var chapterIndex = KcsHskCatalogHtmlParser.ParseChapterIndex(chapterHtml, 2026);
        var heading = Assert.Single(KcsHskCatalogHtmlParser.ParseHeadings(headingHtml));
        var details = KcsHskCatalogHtmlParser.ParseHeadingDetails(detailHtml);

        Assert.Equal(new DateTime(2026, 1, 1), chapterIndex.EffectiveFrom.Date);
        Assert.Equal("제10류 곡물", Assert.Single(chapterIndex.Chapters).KoreanName);
        Assert.Equal("1006", heading.Code);
        Assert.Equal("쌀", heading.KoreanName);
        Assert.Contains(details, entry =>
            entry.Code == "100620"
            && entry.Level == HsCodeLevel.Subheading
            && entry.KoreanName == "현미");
        Assert.Contains(details, entry =>
            entry.Code == "1006201000"
            && entry.Level == HsCodeLevel.National
            && entry.KoreanName == "메현미");
    }

    [Fact]
    public void Chapter_selection_supports_ranges_and_rejects_non_food_scope()
    {
        Assert.Equal([1, 2, 3, 10, 25], KcsHskFoodChapterSelection.Parse("01-03,10,25"));
        Assert.Throws<ArgumentOutOfRangeException>(() => KcsHskFoodChapterSelection.Parse("01,26"));
    }

    [Fact]
    public async Task Import_upserts_official_entries_and_deactivates_replaced_version()
    {
        await using var db = CreateContext();
        db.HsCodeCatalogVersions.Add(new HsCodeCatalogVersion
        {
            StandardCode = "HSK",
            CountryCode = "KR",
            CodeDigits = 10,
            Revision = "2025",
            SourceName = "old",
            SourceUrl = "https://example.invalid/old",
            EffectiveFrom = new DateTime(2025, 1, 1),
            ImportedAtUtc = new DateTime(2025, 1, 1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var source = new StubSource(new KcsHskCatalogSnapshot(
            2026,
            new DateTime(2026, 1, 1),
            [
                new("10", "제10류 곡물", string.Empty, HsCodeLevel.Chapter),
                new("1006", "쌀", "Rice.", HsCodeLevel.Heading),
                new("100620", "현미", "Husked (brown) rice", HsCodeLevel.Subheading),
                new("1006201000", "메현미", new string('N', 600), HsCodeLevel.National)
            ],
            3,
            KcsHskCatalogSource.SourceName,
            KcsHskCatalogSource.SourceUrl));
        var now = new DateTimeOffset(2026, 7, 22, 5, 0, 0, TimeSpan.Zero);
        var service = new KcsHskCatalogImportService(db, source, new FixedTimeProvider(now));

        var imported = await service.ImportAsync(new KcsHskCatalogImportRequest(
            2026,
            [10],
            RequestDelayMilliseconds: 0,
            Force: true));
        var skipped = await service.ImportAsync(new KcsHskCatalogImportRequest(2026, [10]));

        Assert.True(imported.Imported);
        Assert.Equal(4, imported.AddedCount);
        Assert.False(skipped.Imported);
        Assert.Equal(1, source.CallCount);
        var versions = await db.HsCodeCatalogVersions.OrderBy(version => version.Revision).ToArrayAsync();
        Assert.False(versions[0].IsActive);
        Assert.True(versions[1].IsActive);
        var national = await db.HsCodeEntries.SingleAsync(entry => entry.NormalizedCode == "1006201000");
        Assert.Equal("1006.20-1000", national.Code);
        Assert.Equal("100620", national.ParentNormalizedCode);
        Assert.Equal(HsCodeBusinessCategory.Food, national.BusinessCategory);
        Assert.Equal(500, national.EnglishName.Length);
        Assert.True(national.Description.Length > national.EnglishName.Length);
    }

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options,
            new DummyPersonalDataEncryptionService());

    private sealed class StubSource(KcsHskCatalogSnapshot snapshot) : IKcsHskCatalogSource
    {
        public int CallCount { get; private set; }

        public Task<KcsHskCatalogSnapshot> FetchFoodScopeAsync(
            int year,
            IReadOnlyCollection<int> chapters,
            int requestDelayMilliseconds,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(snapshot);
        }
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
