using Ssalddel.Contracts.Common.DeliveryZones;
using Ssalddel.Contracts.Common.Metadata;

namespace 살뜰.도메인.배달권;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformDeliveryZoneLedger,
    SsalddelCodeLayer.Domain,
    "업무 원장별 픽업, 배송, 집결 또는 국내 인계 배달권을 조회 가능한 RDB 투영으로 보존한다.",
    Effects = SsalddelCodeEffect.None,
    FlowOrder = 40,
    Boundary = "업무 원장의 원본을 대체하지 않으며 자동 참여, 자동 배차 또는 계약 확정 상태를 소유하지 않는다.")]
public sealed class 원장배달권투영
{
    public long Id { get; set; }

    public long 배달권Id { get; set; }

    public 플랫폼배달권 배달권 { get; set; } = null!;

    public string 원장유형코드 { get; set; } = 원장배달권원장유형코드.운송원장;

    public string 원장Id { get; set; } = string.Empty;

    public string 역할코드 { get; set; } = 원장배달권역할코드.배송;

    public string 생성근거 { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
