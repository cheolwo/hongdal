using Ssalddel.Contracts.Common.DeliveryZones;

namespace 살뜰.도메인.배달권;

public sealed record 물류위치원본(
    string 위치키,
    string 국가코드,
    string? 시도코드,
    string? 시군구코드,
    string 표시명,
    string 정밀도코드,
    decimal? 위도,
    decimal? 경도,
    string 출처명,
    DateTimeOffset? 검증시각Utc);

/// <summary>
/// 운송·창고 위치를 공개 권역과 보호된 정밀 위치로 나누는 순수 정책입니다.
/// 이 정책은 자동 배차·계약·외부 지오코딩을 수행하지 않습니다.
/// </summary>
public static class 물류위치공개정책
{
    public static 공개물류권역Dto 공개권역만들기(물류위치원본 source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var countryCode = Required(source.국가코드, nameof(source.국가코드), 16);
        var precision = 물류위치공개정밀도코드.전체.Contains(source.정밀도코드, StringComparer.Ordinal)
            ? source.정밀도코드
            : 물류위치공개정밀도코드.국가;
        var latitude = 격자화(source.위도, -90m, 90m);
        var longitude = 격자화(source.경도, -180m, 180m);
        if (latitude is null || longitude is null)
        {
            latitude = null;
            longitude = null;
        }

        return new 공개물류권역Dto
        {
            권역키 = Required(source.위치키, nameof(source.위치키), 120),
            국가코드 = countryCode,
            시도코드 = NormalizeOptional(source.시도코드, 80),
            시군구코드 = NormalizeOptional(source.시군구코드, 80),
            표시명 = Required(source.표시명, nameof(source.표시명), 160),
            정밀도코드 = precision,
            공개대표위도 = latitude,
            공개대표경도 = longitude,
            출처명 = Required(source.출처명, nameof(source.출처명), 160),
            검증시각Utc = source.검증시각Utc
        };
    }

    public static bool 정밀위치허용(
        string 공개범위코드,
        bool 명시적업무권한,
        DateTimeOffset nowUtc,
        DateTimeOffset? 만료시각Utc)
        => 명시적업무권한
           && (string.Equals(공개범위코드, 물류위치공개범위코드.소유자정밀, StringComparison.Ordinal)
               || string.Equals(공개범위코드, 물류위치공개범위코드.참여자정밀, StringComparison.Ordinal)
               || string.Equals(공개범위코드, 물류위치공개범위코드.운영자정밀, StringComparison.Ordinal))
           && (!만료시각Utc.HasValue || 만료시각Utc.Value > nowUtc);

    private static decimal? 격자화(decimal? value, decimal minimum, decimal maximum)
        => value.HasValue && value.Value >= minimum && value.Value <= maximum
            ? decimal.Round(value.Value, 1, MidpointRounding.AwayFromZero)
            : null;

    private static string Required(string? value, string parameterName, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maxLength)
        {
            throw new ArgumentException("필수 값 또는 길이가 올바르지 않습니다.", parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized.Length <= maxLength
                ? normalized
                : throw new ArgumentException("선택 값의 길이가 올바르지 않습니다.");
    }
}
