using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Content;

namespace Ssalddel.Services.Community;

public interface I커뮤니티세계지도뉴스후보UseCase
{
    Task<커뮤니티세계지도뉴스후보Response?> 조회Async(
        string observationStableId,
        string? sourceKey,
        int take,
        CancellationToken cancellationToken = default);
}

public sealed record 국가별뉴스출처Rss연결Definition(
    string PublisherKey,
    커뮤니티세계지도뉴스Feed상태Dto PublisherFeedStatus,
    IReadOnlyList<string> RelatedOfficialSourceKeys,
    string RelationNotice);

public static class 국가별뉴스출처Rss연결Catalog
{
    private static readonly DateTimeOffset VerifiedAtUtc =
        new(2026, 8, 4, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<국가별뉴스출처Rss연결Definition> All { get; } =
    [
        new(
            "kr-yonhap",
            new 커뮤니티세계지도뉴스Feed상태Dto(
                커뮤니티세계지도뉴스Feed상태Codes.OfficialPublicFeedUnverified,
                "언론사 공식 공개 RSS 미확인",
                "연합뉴스 공식 홈페이지는 연결하지만 현재 검증된 공개 RSS endpoint는 등록하지 않았습니다.",
                "https://www.yna.co.kr/",
                VerifiedAtUtc),
            [
                CommunityInformationSourceKeys.MafraPressReleases,
                CommunityInformationSourceKeys.MafraExplanations,
                CommunityInformationSourceKeys.MfdsPressReleases
            ],
            "아래 후보는 연합뉴스 기사가 아니라 대한민국 농림축산식품부·식품의약품안전처의 별도 공식 RSS 자료입니다."),
        new(
            "us-associated-press",
            new 커뮤니티세계지도뉴스Feed상태Dto(
                커뮤니티세계지도뉴스Feed상태Codes.LicensedApiOnly,
                "공개 RSS 대신 라이선스형 API",
                "AP가 문서화한 feed는 AP Media API의 JSON feed이며 공개 RSS 수집 대상으로 등록하지 않았습니다.",
                "https://api.ap.org/media/v/docs/Feeds_and_Linked_Content.htm",
                VerifiedAtUtc),
            [],
            "현재 이 마커에 연결한 별도 공식뉴스 RSS 원천이 없습니다."),
        new(
            "cn-xinhua",
            new 커뮤니티세계지도뉴스Feed상태Dto(
                커뮤니티세계지도뉴스Feed상태Codes.OfficialPublicFeedUnverified,
                "언론사 공식 공개 RSS 미확인",
                "신화통신 공식 홈페이지는 연결하지만 현재 검증된 공개 RSS endpoint는 등록하지 않았습니다.",
                "https://english.news.cn/",
                VerifiedAtUtc),
            [],
            "현재 이 마커에 연결한 별도 공식뉴스 RSS 원천이 없습니다."),
        new(
            "au-abc-news",
            new 커뮤니티세계지도뉴스Feed상태Dto(
                커뮤니티세계지도뉴스Feed상태Codes.Discontinued,
                "RSS 갱신 중단",
                "ABC는 뉴스 RSS feed가 더 이상 갱신되지 않는다고 공식 안내하므로 수집 원천으로 등록하지 않았습니다.",
                "https://help.abc.net.au/hc/en-us/articles/6147104938383-Why-are-RSS-feeds-no-longer-being-updated",
                VerifiedAtUtc),
            [],
            "현재 이 마커에 연결한 별도 공식뉴스 RSS 원천이 없습니다.")
    ];

    public static 국가별뉴스출처Rss연결Definition? Find(string publisherKey)
        => All.FirstOrDefault(item => string.Equals(
            item.PublisherKey,
            publisherKey,
            StringComparison.OrdinalIgnoreCase));
}

public sealed class 커뮤니티세계지도뉴스후보UseCase(
    ICommunityInformationCollectionService informationCollectionService,
    I공식뉴스검토원장Service reviewLedgerService,
    TimeProvider timeProvider) : I커뮤니티세계지도뉴스후보UseCase
{
    public async Task<커뮤니티세계지도뉴스후보Response?> 조회Async(
        string observationStableId,
        string? sourceKey,
        int take,
        CancellationToken cancellationToken = default)
    {
        var stableId = observationStableId?.Trim();
        const string prefix = "news-publisher:";
        if (string.IsNullOrWhiteSpace(stableId)
            || !stableId.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var publisherKey = stableId[prefix.Length..];
        var publisher = 국가별뉴스출처MapCatalog.All.FirstOrDefault(item => string.Equals(
            item.Key,
            publisherKey,
            StringComparison.OrdinalIgnoreCase));
        var relation = 국가별뉴스출처Rss연결Catalog.Find(publisherKey);
        if (publisher is null || relation is null)
        {
            return null;
        }

        var availableSources = informationCollectionService.GetSources()
            .Where(item => relation.RelatedOfficialSourceKeys.Contains(
                item.SourceKey,
                StringComparer.OrdinalIgnoreCase))
            .OrderBy(item => item.DisplayName, StringComparer.Ordinal)
            .ToArray();
        var normalizedSourceKey = NormalizeOptional(sourceKey);
        if (normalizedSourceKey is not null
            && !relation.RelatedOfficialSourceKeys.Contains(
                normalizedSourceKey,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "이 지도 마커에 연결된 공식뉴스 RSS sourceKey가 아닙니다.",
                nameof(sourceKey));
        }

        if (normalizedSourceKey is null)
        {
            return BuildResponse(
                stableId,
                publisher,
                relation,
                availableSources,
                null,
                timeProvider.GetUtcNow().UtcDateTime,
                [],
                []);
        }

        var approvedLedgers = await reviewLedgerService.목록Async(
            [normalizedSourceKey],
            CommunityInformationReviewStates.Approved,
            Math.Clamp(take, 1, 20),
            cancellationToken);
        return BuildResponse(
            stableId,
            publisher,
            relation,
            availableSources,
            normalizedSourceKey,
            timeProvider.GetUtcNow().UtcDateTime,
            approvedLedgers.Select(item => item.Candidate).ToArray(),
            []);
    }

    private static 커뮤니티세계지도뉴스후보Response BuildResponse(
        string stableId,
        국가별뉴스출처MapDefinition publisher,
        국가별뉴스출처Rss연결Definition relation,
        IReadOnlyList<CommunityInformationSourceDto> availableSources,
        string? selectedSourceKey,
        DateTime generatedAtUtc,
        IReadOnlyList<CommunityInformationCandidateDto> items,
        IReadOnlyList<CommunityInformationSourceFailureDto> failures)
        => new(
            stableId,
            publisher.DisplayName,
            publisher.CountryCode,
            relation.PublisherFeedStatus,
            availableSources,
            selectedSourceKey,
            generatedAtUtc,
            items,
            failures,
            RequiresExplicitSourceSelection: true,
            CreatesPost: false,
            relation.RelationNotice,
            "운영 검토 원장에서 승인된 후보의 제목·요약·원문 링크만 표시합니다. 기사 정확성·정치적 중립성·가격·재고·공급 가능성을 확정하지 않으며 자동 게시하지 않습니다.");

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
