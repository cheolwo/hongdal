using System.Globalization;
using Hongdal.Contracts.Common.Content;
using Hongdal.Domain.AgriculturalFisheries;
using Hongdal.Infrastructure.Persistence.AgriculturalFisheries;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Services.Content;

public sealed class CommunityKamisInformationCandidateSource : ICommunityInformationCandidateSource
{
    private const string DocumentationUrl = "https://www.kamis.or.kr/customer/reference/openapi_list.do";
    private readonly AgriculturalFisheriesDbContext _db;

    public CommunityKamisInformationCandidateSource(AgriculturalFisheriesDbContext db)
    {
        _db = db;
    }

    public CommunityInformationSourceDto Source { get; } = new(
        CommunityInformationSourceKeys.KamisPriceObservations,
        CommunityInformationSourceTypes.PublicData,
        "KAMIS 농산물 유통정보",
        "농수산물 가격 관측값",
        CommunityInformationCollectionModes.ScheduledArchive,
        "일별 수집, KAMIS 제공일 기준",
        "공식 관측값이어도 품목·품종·등급·단위를 확인한 편집 결과만 커뮤니티 정보 글로 게시합니다.",
        DocumentationUrl,
        true);

    public async Task<IReadOnlyList<CommunityInformationCandidateDto>> ReadAsync(
        CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (!MatchesKoreanSource(query.CountryCode)
            || !MatchesOfficialObservation(query.ReviewState))
        {
            return [];
        }

        var observations = _db.KamisPriceObservations
            .AsNoTracking()
            .Where(observation => observation.FrequencyCode == "Daily"
                                  && !observation.IsPriceMissing
                                  && observation.PriceKrw.HasValue);
        var searchText = query.SearchText?.Trim();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            observations = observations.Where(observation =>
                observation.ItemName.Contains(searchText)
                || observation.KindName.Contains(searchText)
                || observation.CategoryName.Contains(searchText)
                || observation.RankName.Contains(searchText));
        }

        var fetchCount = (int)Math.Clamp(Math.Max((long)query.Take * 4, 50), 1, 200);
        var rows = await observations
            .OrderByDescending(observation => observation.SurveyDate)
            .ThenBy(observation => observation.CategoryName)
            .ThenBy(observation => observation.ItemName)
            .ThenBy(observation => observation.KindName)
            .ThenBy(observation => observation.RankName)
            .Take(fetchCount)
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(observation => observation.RecordKey, StringComparer.Ordinal)
            .Select(group => ToCandidate(group.First()))
            .Take(Math.Clamp(query.Take, 1, 100))
            .ToArray();
    }

    private static CommunityInformationCandidateDto ToCandidate(KamisPriceObservation observation)
    {
        var specification = string.Join(
            " · ",
            new[] { observation.KindName, observation.RankName }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        var summaryParts = new[]
        {
            observation.ProductClassName,
            observation.CategoryName,
            $"{observation.PriceKrw!.Value.ToString("N0", CultureInfo.InvariantCulture)}원/{observation.Unit}"
        };
        var sourceUrl = IsPublicHttpUrl(observation.SourceUrl)
            ? observation.SourceUrl.Trim()
            : DocumentationUrl;

        return new CommunityInformationCandidateDto(
            $"kamis:{observation.RecordKey}",
            CommunityInformationSourceKeys.KamisPriceObservations,
            CommunityInformationSourceTypes.PublicData,
            "KAMIS 농산물 유통정보",
            $"{observation.ItemName}{(specification.Length == 0 ? string.Empty : $" ({specification})")}",
            string.Join(" · ", summaryParts.Where(value => !string.IsNullOrWhiteSpace(value))),
            sourceUrl,
            null,
            null,
            observation.SurveyDate,
            DateTime.SpecifyKind(observation.LastSeenAtUtc, DateTimeKind.Utc),
            "KR",
            "ko",
            "KRW",
            string.IsNullOrWhiteSpace(observation.Unit) ? null : observation.Unit.Trim(),
            CommunityInformationReviewStates.OfficialObservation,
            new[] { "농수산물", observation.CategoryName, observation.ProductClassName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            "KAMIS Open API에서 수집해 보관한 공식 가격 관측값입니다.",
            "전체 시장 평균이나 판매 권고가 아닙니다. 조사일, 품종, 등급, 단위와 조사처가 다른 값은 같은 가격처럼 직접 비교할 수 없습니다.");
    }

    private static bool MatchesKoreanSource(string? countryCode)
        => string.IsNullOrWhiteSpace(countryCode)
           || string.Equals(countryCode.Trim(), "KR", StringComparison.OrdinalIgnoreCase);

    private static bool MatchesOfficialObservation(string? reviewState)
        => string.IsNullOrWhiteSpace(reviewState)
           || string.Equals(
               reviewState.Trim(),
               CommunityInformationReviewStates.OfficialObservation,
               StringComparison.OrdinalIgnoreCase);

    private static bool IsPublicHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
