namespace Ssalddel.Contracts.Common.DeliveryZones;

/// <summary>지도와 목록에 제공하는 물류 위치의 공개 범위를 구분합니다.</summary>
public static class 물류위치공개범위코드
{
    public const string 공개권역 = "public-region";
    public const string 소유자정밀 = "owner-precision";
    public const string 참여자정밀 = "participant-precision";
    public const string 운영자정밀 = "operator-precision";

    public static IReadOnlySet<string> 전체 { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        공개권역,
        소유자정밀,
        참여자정밀,
        운영자정밀
    };
}

/// <summary>공개 지도에 허용되는 대표점의 의미를 나타냅니다.</summary>
public static class 물류위치공개정밀도코드
{
    public const string 국가 = "country";
    public const string 시도 = "province";
    public const string 시군구 = "city-county";

    public static IReadOnlySet<string> 전체 { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        국가,
        시도,
        시군구
    };
}

/// <summary>
/// 공개 응답 전용 위치입니다. 주소, 연락처, 정밀 좌표를 포함하지 않으며 대표점은 격자화된 값만 담습니다.
/// </summary>
public sealed class 공개물류권역Dto
{
    public string 권역키 { get; set; } = string.Empty;
    public string 국가코드 { get; set; } = string.Empty;
    public string? 시도코드 { get; set; }
    public string? 시군구코드 { get; set; }
    public string 표시명 { get; set; } = string.Empty;
    public string 정밀도코드 { get; set; } = 물류위치공개정밀도코드.국가;
    public decimal? 공개대표위도 { get; set; }
    public decimal? 공개대표경도 { get; set; }
    public string 출처명 { get; set; } = string.Empty;
    public DateTimeOffset? 검증시각Utc { get; set; }
    public string 공개한계 { get; set; } = "권역 대표점은 실제 상하차지나 시설 위치가 아닙니다.";
}

/// <summary>권한이 확인된 소유자·참여자·운영자에게만 제한적으로 제공하는 정밀 위치입니다.</summary>
public sealed class 참여자물류정밀위치Dto
{
    public string 위치키 { get; set; } = string.Empty;
    public decimal 위도 { get; set; }
    public decimal 경도 { get; set; }
    public string 공개범위코드 { get; set; } = 물류위치공개범위코드.참여자정밀;
    public DateTimeOffset 유효시각Utc { get; set; }
    public DateTimeOffset? 만료시각Utc { get; set; }
    public string 노출근거 { get; set; } = string.Empty;
}
