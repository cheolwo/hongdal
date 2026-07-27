using Ssalddel.Contracts.Common.Metadata;

namespace 살뜰.도메인.배달권;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformDeliveryZoneLedger,
    SsalddelCodeLayer.Domain,
    "여러 업무 원장이 공유하는 배달권의 안정 식별자와 행정·좌표 판정 근거를 보존한다.",
    Effects = SsalddelCodeEffect.None,
    FlowOrder = 30,
    Boundary = "상세 주소나 참여자 정보는 보존하지 않고 배달권 범위의 대표 정보만 가진다.")]
public sealed class 플랫폼배달권
{
    public long Id { get; set; }

    public string 배달권키 { get; set; } = string.Empty;

    public string 배달권명 { get; set; } = string.Empty;

    public string 판정방식 { get; set; } = string.Empty;

    public string? 법정동코드 { get; set; }

    public string? 시도명 { get; set; }

    public string? 시군구명 { get; set; }

    public string? 대표건물명 { get; set; }

    public string? 대표건물주소 { get; set; }

    public decimal? 대표위도 { get; set; }

    public decimal? 대표경도 { get; set; }

    public string 인접배달권키Json { get; set; } = "[]";

    public bool 활성 { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
