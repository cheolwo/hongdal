using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.DeliveryZones;

public static class 원장배달권원장유형코드
{
    public const string 음식주문 = "FoodOrder";
    public const string 마트주문 = "MartOrder";
    public const string 같이주문 = "TogetherOrder";
    public const string 같이수입 = "TogetherImport";
    public const string 운송원장 = "TransportLedger";

    public static IReadOnlyList<string> 전체 { get; } =
    [
        음식주문,
        마트주문,
        같이주문,
        같이수입,
        운송원장
    ];

    public static bool 지원여부(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && 전체.Contains(value.Trim(), StringComparer.Ordinal);
}

public static class 원장배달권역할코드
{
    public const string 픽업 = "Pickup";
    public const string 배송 = "Delivery";
    public const string 집결 = "Assembly";
    public const string 국내인계 = "DomesticHandoff";

    public static IReadOnlyList<string> 전체 { get; } =
    [
        픽업,
        배송,
        집결,
        국내인계
    ];

    public static bool 지원여부(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && 전체.Contains(value.Trim(), StringComparer.Ordinal);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformDeliveryZoneLedger,
    SsalddelCodeLayer.Contract,
    "음식 주문, 마트 주문, 같이 주문, 같이 수입과 운송 원장을 같은 배달권 의미로 연결한다.",
    FlowOrder = 10,
    Boundary = "배달권은 수요 집계와 물류 후보 범위이며 자동 참여, 자동 배차 또는 계약 확정 근거가 아니다.")]
public sealed class 원장배달권연결요청
{
    public string 원장유형코드 { get; set; } = string.Empty;

    public string 원장Id { get; set; } = string.Empty;

    public string 역할코드 { get; set; } = 원장배달권역할코드.배송;

    public string? 기존배송권키 { get; set; }

    public string? 기존배송권명 { get; set; }

    public string? 기존배송권판정방식 { get; set; }

    public IReadOnlyList<string> 기존인접배송권키목록 { get; set; } = [];

    public bool 기존연결우선여부 { get; set; }

    public string? 도로명주소 { get; set; }

    public decimal? 위도 { get; set; }

    public decimal? 경도 { get; set; }

    public string 생성근거 { get; set; } = string.Empty;
}

public sealed class 플랫폼배달권Dto
{
    public string 배달권키 { get; set; } = string.Empty;

    public string 배달권명 { get; set; } = string.Empty;

    public string 판정방식 { get; set; } = string.Empty;

    public string? 법정동코드 { get; set; }

    public string? 시도명 { get; set; }

    public string? 시군구명 { get; set; }

    public decimal? 대표위도 { get; set; }

    public decimal? 대표경도 { get; set; }

    public IReadOnlyList<string> 인접배달권키목록 { get; set; } = [];
}

public sealed class 원장배달권연결Dto
{
    public string 원장유형코드 { get; set; } = string.Empty;

    public string 원장Id { get; set; } = string.Empty;

    public string 역할코드 { get; set; } = string.Empty;

    public string 생성근거 { get; set; } = string.Empty;

    public 플랫폼배달권Dto 배달권 { get; set; } = new();

    public DateTime UpdatedAtUtc { get; set; }
}
