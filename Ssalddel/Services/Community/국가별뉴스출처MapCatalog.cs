using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Services.Community;

public sealed record 국가별뉴스출처MapDefinition(
    string Key,
    string CountryCode,
    string CountryName,
    string DisplayName,
    string OrganizationTypeCode,
    string OrganizationTypeLabel,
    double Latitude,
    double Longitude,
    string HomepageUrl,
    string AboutUrl,
    DateTimeOffset VerifiedAtUtc,
    string Description);

/// <summary>
/// 국가별 뉴스 조직의 존재와 공식 홈페이지를 지도에서 찾기 위한 공개 카탈로그입니다.
/// 좌표는 본사·취재국 위치가 아니라 국가 대표점입니다.
/// </summary>
public static class 국가별뉴스출처MapCatalog
{
    public const string DatasetKey = "country-news-publisher-catalog-v1";

    public static IReadOnlyList<국가별뉴스출처MapDefinition> All { get; } =
    [
        new(
            "kr-yonhap",
            "KR",
            "대한민국",
            "연합뉴스",
            커뮤니티세계지도뉴스출처유형Codes.NewsAgency,
            "뉴스통신사",
            36.9,
            127.2,
            "https://www.yna.co.kr/",
            "https://en.yna.co.kr/aboutus/index",
            new DateTimeOffset(2026, 8, 4, 0, 0, 0, TimeSpan.Zero),
            "한국의 뉴스통신사 공식 홈페이지를 연결합니다."),
        new(
            "us-associated-press",
            "US",
            "미국",
            "Associated Press",
            커뮤니티세계지도뉴스출처유형Codes.NewsCooperative,
            "비영리 뉴스 협동조합",
            40.5,
            -99.2,
            "https://apnews.com/",
            "https://www.ap.org/about/",
            new DateTimeOffset(2026, 8, 4, 0, 0, 0, TimeSpan.Zero),
            "미국에서 설립된 비영리 뉴스 협동조합의 공식 페이지를 연결합니다."),
        new(
            "cn-xinhua",
            "CN",
            "중국",
            "Xinhua News Agency",
            커뮤니티세계지도뉴스출처유형Codes.StateNewsAgency,
            "국가통신사",
            36.7,
            105.0,
            "https://english.news.cn/",
            "https://www.news.cn/xinhuashe/jbqk.htm",
            new DateTimeOffset(2026, 8, 4, 0, 0, 0, TimeSpan.Zero),
            "중국 국가통신사 신화통신의 공식 페이지를 연결합니다."),
        new(
            "au-abc-news",
            "AU",
            "호주",
            "ABC News Australia",
            커뮤니티세계지도뉴스출처유형Codes.PublicServiceMedia,
            "공영미디어 뉴스",
            -24.5,
            134.5,
            "https://www.abc.net.au/news/",
            "https://www.abc.net.au/news/about-abc-news",
            new DateTimeOffset(2026, 8, 4, 0, 0, 0, TimeSpan.Zero),
            "호주 공영미디어 ABC의 뉴스 공식 페이지를 연결합니다.")
    ];

    public static IReadOnlyList<국가별뉴스출처MapDefinition> ForCountry(string countryCode)
        => All.Where(item => string.Equals(
                item.CountryCode,
                countryCode,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

    public static 커뮤니티세계지도ObservationDto ToObservation(
        국가별뉴스출처MapDefinition source)
        => new(
            $"news-publisher:{source.Key}",
            CommunityPageRoutes.WorldMapDayWorkDataset,
            커뮤니티세계지도LayerCodes.NewsPublisher,
            source.CountryCode,
            source.CountryName,
            source.Latitude,
            source.Longitude,
            source.DisplayName,
            $"{source.OrganizationTypeLabel} · {source.Description} 지도 좌표는 국가 대표점이며 본사·취재국·기사 발생 위치가 아닙니다.",
            $"{source.DisplayName} 공식 소개",
            source.VerifiedAtUtc,
            커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
            source.HomepageUrl,
            source.AboutUrl,
            커뮤니티세계지도위치정밀도Codes.CountryRepresentative,
            MarkerStatusCode: source.OrganizationTypeCode,
            SourceDatasetKey: DatasetKey,
            SourceUpdatedAtUtc: source.VerifiedAtUtc,
            UpdateCycle: "수동 검토",
            FreshnessCode: 커뮤니티세계지도FreshnessCodes.Fresh,
            BoundaryNotice: "대표 뉴스 조직 한 곳을 예시로 연결한 카탈로그이며 국가 전체 언론을 대표하거나 신뢰도·정치적 중립성·기사 정확성을 보증하지 않습니다.");
}
