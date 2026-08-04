using Ssalddel.Contracts.Common.DeliveryZones;
using 살뜰.도메인.배달권;

namespace 살뜰.Services.DeliveryZones;

public static class 원장배달권공개투영어댑터
{
    public const string 기본국가코드 = "KR";
    public const string 기본노출근거 = "platform-delivery-zone";

    public static 공개물류권역Dto 공개권역변환(
        플랫폼배달권Dto source,
        string 국가코드,
        string? 노출근거 = null,
        DateTimeOffset? 검증시각Utc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sourceName = NormalizeRequired(source.배달권명, nameof(source.배달권명), 160);
        var countryCode = NormalizeRequired(국가코드, nameof(국가코드), 16);
        var precision = ResolvePrecision(source.시도명, source.시군구명);
        var zoneKey = $"{기본노출근거}:{NormalizeRequired(source.배달권키, nameof(source.배달권키), 120)}";
        return 물류위치공개정책.공개권역만들기(new 물류위치원본(
            zoneKey,
            countryCode,
            null,
            null,
            sourceName,
            precision,
            source.대표위도,
            source.대표경도,
            string.IsNullOrWhiteSpace(노출근거)
                ? 기본노출근거
                : NormalizeRequired(노출근거, nameof(노출근거), 160),
            검증시각Utc));
    }

    public static 참여자물류정밀위치Dto? 참여자정밀위치변환(
        플랫폼배달권Dto source,
        string 공개범위코드,
        bool 명시적업무권한,
        DateTimeOffset nowUtc,
        DateTimeOffset? 만료시각Utc,
        string? 노출근거 = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!물류위치공개정책.정밀위치허용(공개범위코드, 명시적업무권한, nowUtc, 만료시각Utc))
        {
            return null;
        }

        if (!source.대표위도.HasValue || !source.대표경도.HasValue)
        {
            return null;
        }

        return new 참여자물류정밀위치Dto
        {
            위치키 = $"{기본노출근거}:{NormalizeRequired(source.배달권키, nameof(source.배달권키), 120)}",
            위도 = source.대표위도.Value,
            경도 = source.대표경도.Value,
            공개범위코드 = 공개범위코드,
            유효시각Utc = nowUtc,
            만료시각Utc = 만료시각Utc,
            노출근거 = string.IsNullOrWhiteSpace(노출근거)
                ? 기본노출근거
                : NormalizeRequired(노출근거, nameof(노출근거), 160)
        };
    }

    private static string ResolvePrecision(string? provinceName, string? cityCountyName)
        => !string.IsNullOrWhiteSpace(cityCountyName)
            ? 물류위치공개정밀도코드.시군구
            : !string.IsNullOrWhiteSpace(provinceName)
                ? 물류위치공개정밀도코드.시도
                : 물류위치공개정밀도코드.국가;

    private static string NormalizeRequired(string? value, string parameterName, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("필수 값이 비어 있거나 공백입니다.", parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"최대 {maxLength}자까지 입력할 수 있습니다.", parameterName);
        }

        return normalized;
    }
}
