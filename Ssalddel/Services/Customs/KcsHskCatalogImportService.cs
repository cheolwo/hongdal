using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.HsCodes;

namespace Ssalddel.Services.Customs;

public sealed record KcsHskCatalogImportRequest(
    int Year,
    IReadOnlyList<int>? Chapters = null,
    int RequestDelayMilliseconds = 150,
    bool Force = false);

public sealed record KcsHskCatalogImportResult(
    long CatalogVersionId,
    bool Imported,
    int EntryCount,
    int AddedCount,
    int UpdatedCount,
    int DeactivatedCount,
    int RequestCount,
    DateTime EffectiveFrom,
    DateTime ImportedAtUtc,
    string SourceUrl);

public interface IKcsHskCatalogImportService
{
    Task<KcsHskCatalogImportResult> ImportAsync(
        KcsHskCatalogImportRequest request,
        CancellationToken cancellationToken = default);
}

internal sealed class KcsHskCatalogImportService(
    SsalddelContext db,
    IKcsHskCatalogSource source,
    TimeProvider timeProvider) : IKcsHskCatalogImportService
{
    private const string StandardCode = "HSK";
    private const string CountryCode = "KR";

    public async Task<KcsHskCatalogImportResult> ImportAsync(
        KcsHskCatalogImportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Year is < 2000 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "HSK 적용 연도는 2000~2100 범위여야 합니다.");
        }

        var chapters = KcsHskFoodChapterSelection.Normalize(request.Chapters);
        var revision = request.Year.ToString(CultureInfo.InvariantCulture);
        var existingVersion = await db.HsCodeCatalogVersions
            .Include(version => version.Entries)
            .FirstOrDefaultAsync(version =>
                    version.StandardCode == StandardCode
                    && version.CountryCode == CountryCode
                    && version.Revision == revision
                    && version.CodeDigits == 10,
                cancellationToken);
        if (!request.Force
            && existingVersion is { IsActive: true }
            && HasRequestedCoverage(existingVersion.Entries, chapters))
        {
            return new KcsHskCatalogImportResult(
                existingVersion.Id,
                Imported: false,
                existingVersion.Entries.Count(entry => entry.IsActive),
                AddedCount: 0,
                UpdatedCount: 0,
                DeactivatedCount: 0,
                RequestCount: 0,
                existingVersion.EffectiveFrom,
                existingVersion.ImportedAtUtc,
                existingVersion.SourceUrl);
        }

        var snapshot = await source.FetchFoodScopeAsync(
            request.Year,
            chapters,
            request.RequestDelayMilliseconds,
            cancellationToken);
        ValidateSnapshot(snapshot, chapters);

        var importedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var catalogVersion = existingVersion ?? new HsCodeCatalogVersion
        {
            StandardCode = StandardCode,
            CountryCode = CountryCode,
            Revision = revision,
            CodeDigits = 10
        };
        if (existingVersion is null)
        {
            db.HsCodeCatalogVersions.Add(catalogVersion);
        }

        catalogVersion.SourceName = snapshot.SourceName;
        catalogVersion.SourceUrl = snapshot.SourceUrl;
        catalogVersion.EffectiveFrom = snapshot.EffectiveFrom;
        catalogVersion.EffectiveTo = null;
        catalogVersion.IsActive = true;
        catalogVersion.ImportedAtUtc = importedAtUtc;
        var existingByCode = catalogVersion.Entries
            .ToDictionary(entry => entry.NormalizedCode, StringComparer.Ordinal);
        var sourceCodes = snapshot.Entries
            .Select(entry => entry.Code)
            .ToHashSet(StringComparer.Ordinal);
        var addedCount = 0;
        var updatedCount = 0;
        foreach (var sourceEntry in snapshot.Entries)
        {
            if (!existingByCode.TryGetValue(sourceEntry.Code, out var entry))
            {
                entry = new HsCodeEntry
                {
                    CatalogVersion = catalogVersion,
                    NormalizedCode = sourceEntry.Code,
                    CreatedAtUtc = importedAtUtc
                };
                catalogVersion.Entries.Add(entry);
                existingByCode.Add(sourceEntry.Code, entry);
                addedCount++;
            }
            else
            {
                updatedCount++;
            }

            Apply(entry, sourceEntry, sourceCodes, importedAtUtc);
        }

        var deactivatedCount = 0;
        foreach (var entry in catalogVersion.Entries.Where(entry =>
                     entry.IsActive
                     && IsRequestedChapter(entry.NormalizedCode, chapters)
                     && !sourceCodes.Contains(entry.NormalizedCode)))
        {
            entry.IsActive = false;
            entry.UpdatedAtUtc = importedAtUtc;
            deactivatedCount++;
        }

        var activeChapterCodes = catalogVersion.Entries
            .Where(entry => entry.IsActive && entry.Level == HsCodeLevel.Chapter)
            .Select(entry => entry.NormalizedCode)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();
        catalogVersion.Notes =
            $"관세청 {request.Year}년 HSK 식품 재료 후보 탐색 범위: {string.Join(",", activeChapterCodes)} " +
            "(제25류는 제2501호만 수집). 자동 품목분류 확정값이 아니며 실제 신고 전 물품 특성 확인과 전문가 검토가 필요합니다.";

        var replacedVersions = await db.HsCodeCatalogVersions
            .Where(version =>
                version.Id != catalogVersion.Id
                && version.StandardCode == StandardCode
                && version.CountryCode == CountryCode
                && version.IsActive)
            .ToArrayAsync(cancellationToken);
        foreach (var replacedVersion in replacedVersions)
        {
            replacedVersion.IsActive = false;
            replacedVersion.EffectiveTo = snapshot.EffectiveFrom.AddDays(-1);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new KcsHskCatalogImportResult(
            catalogVersion.Id,
            Imported: true,
            catalogVersion.Entries.Count(entry => entry.IsActive),
            addedCount,
            updatedCount,
            deactivatedCount,
            snapshot.RequestCount,
            snapshot.EffectiveFrom,
            importedAtUtc,
            snapshot.SourceUrl);
    }

    private static void Apply(
        HsCodeEntry target,
        KcsHskCatalogSourceEntry source,
        IReadOnlySet<string> allCodes,
        DateTime importedAtUtc)
    {
        var category = HsCodeBusinessCategoryClassifier.Classify(source.Code);
        var description = string.Join(
            " / ",
            new[] { source.KoreanName, source.EnglishName }.Where(value => !string.IsNullOrWhiteSpace(value)));
        target.Code = FormatCode(source.Code);
        target.ParentNormalizedCode = ResolveParent(source.Code, allCodes);
        target.Level = source.Level;
        target.KoreanName = Truncate(source.KoreanName, 500);
        target.EnglishName = Truncate(source.EnglishName, 500);
        target.Description = Truncate(description, 4000);
        target.SearchKeywords = Truncate(description, 4000);
        target.BusinessCategory = category.Category;
        target.BusinessCategoryReason = category.Reason;
        target.IsActive = true;
        target.UpdatedAtUtc = importedAtUtc;
    }

    private static string Truncate(string value, int maximumLength)
        => value.Length <= maximumLength ? value : value[..maximumLength];

    private static bool HasRequestedCoverage(
        IEnumerable<HsCodeEntry> entries,
        IReadOnlyCollection<int> requestedChapters)
    {
        var activeChapters = entries
            .Where(entry => entry.IsActive && entry.Level == HsCodeLevel.Chapter)
            .Select(entry => entry.NormalizedCode)
            .ToHashSet(StringComparer.Ordinal);
        return requestedChapters.All(chapter =>
                   activeChapters.Contains(chapter.ToString("00", CultureInfo.InvariantCulture)))
               && entries.Any(entry => entry.IsActive && entry.Level == HsCodeLevel.National);
    }

    private static bool IsRequestedChapter(
        string normalizedCode,
        IReadOnlyCollection<int> requestedChapters)
    {
        if (normalizedCode.Length < 2
            || !int.TryParse(normalizedCode[..2], NumberStyles.None, CultureInfo.InvariantCulture, out var chapter))
        {
            return false;
        }

        return requestedChapters.Contains(chapter);
    }

    private static string? ResolveParent(string code, IReadOnlySet<string> allCodes)
        => code.Length switch
        {
            2 => null,
            4 => code[..2],
            6 => code[..4],
            10 when allCodes.Contains(code[..6]) => code[..6],
            10 => code[..4],
            _ => null
        };

    private static string FormatCode(string code)
        => code.Length switch
        {
            6 => $"{code[..4]}.{code[4..]}",
            10 => $"{code[..4]}.{code[4..6]}-{code[6..]}",
            _ => code
        };

    private static void ValidateSnapshot(
        KcsHskCatalogSnapshot snapshot,
        IReadOnlyCollection<int> requestedChapters)
    {
        if (snapshot.Entries.Count == 0)
        {
            throw new InvalidOperationException("관세청 HSK 응답이 비어 있어 기존 카탈로그를 유지합니다.");
        }

        var foundChapters = snapshot.Entries
            .Where(entry => entry.Level == HsCodeLevel.Chapter)
            .Select(entry => int.Parse(entry.Code, CultureInfo.InvariantCulture))
            .ToHashSet();
        var missingChapters = requestedChapters.Where(chapter => !foundChapters.Contains(chapter)).ToArray();
        if (missingChapters.Length > 0)
        {
            throw new InvalidOperationException(
                $"관세청 HSK 응답에 요청한 류가 없습니다: {string.Join(", ", missingChapters.Select(value => value.ToString("00", CultureInfo.InvariantCulture)))}");
        }

        if (!snapshot.Entries.Any(entry => entry.Level == HsCodeLevel.National))
        {
            throw new InvalidOperationException("10자리 HSK 품목이 없어 기존 카탈로그를 유지합니다.");
        }
    }
}

public static class KcsHskFoodChapterSelection
{
    public static IReadOnlyList<int> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Normalize(null);
        }

        var chapters = new SortedSet<int>();
        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var range = token.Split('-', StringSplitOptions.TrimEntries);
            if (range.Length == 1 && int.TryParse(range[0], out var chapter))
            {
                chapters.Add(chapter);
                continue;
            }

            if (range.Length == 2
                && int.TryParse(range[0], out var start)
                && int.TryParse(range[1], out var end)
                && start <= end)
            {
                foreach (var item in Enumerable.Range(start, end - start + 1))
                {
                    chapters.Add(item);
                }

                continue;
            }

            throw new ArgumentException($"HSK 류 범위 형식이 올바르지 않습니다: {token}", nameof(value));
        }

        return Normalize(chapters.ToArray());
    }

    public static IReadOnlyList<int> Normalize(IReadOnlyCollection<int>? chapters)
    {
        var normalized = (chapters is { Count: > 0 }
                ? chapters
                : Enumerable.Range(1, 24).Append(25))
            .Distinct()
            .OrderBy(chapter => chapter)
            .ToArray();
        if (normalized.Any(chapter => chapter is < 1 or > 25))
        {
            throw new ArgumentOutOfRangeException(
                nameof(chapters),
                "식품 HSK 수집 범위는 제01~25류만 지원하며 제25류에서는 제2501호만 가져옵니다.");
        }

        return normalized;
    }
}
