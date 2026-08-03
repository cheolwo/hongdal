using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Services.Content;

public interface I홍익학당철학영상MapLayer조회UseCase
{
    Task<HongikAcademyContentMapLayerResponse> 조회Async(
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.HongikAcademyContentMap,
    SsalddelCodeLayer.Application,
    "홍익학당 철학·영상 지리 레코드의 등록·검증 상태를 읽기 전용 지도 레이어로 조립",
    ContractType = typeof(I홍익학당철학영상MapLayer조회UseCase),
    FlowOrder = 30,
    Boundary = "현재는 저장소를 읽지 않는 명시적 빈 상태만 공개하며 개인·시설·추정 좌표를 만들거나 외부 영상을 호출하지 않습니다.")]
public sealed class 홍익학당철학영상MapLayer조회UseCase(
    TimeProvider timeProvider) : I홍익학당철학영상MapLayer조회UseCase
{
    public Task<HongikAcademyContentMapLayerResponse> 조회Async(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new HongikAcademyContentMapLayerResponse(
            HongikAcademyContentMapLayerKeys.PhilosophyVideo,
            "홍익학당 철학·영상",
            "야간 사유·영상 탐색",
            HongikAcademyContentMapProvenanceStatusCodes.NoVerifiedGeographicRecords,
            "검증된 지리 기록 없음",
            false,
            0,
            timeProvider.GetUtcNow().UtcDateTime,
            [
                new HongikAcademyContentMapSourceDto(
                    "community-prajna-publication-policy",
                    "반야 관리자 선별 공개 기준",
                    HongikAcademyContentMapProvenanceStatusCodes.NoVerifiedGeographicRecords,
                    null,
                    0,
                    "이 공개 기준은 콘텐츠 원천·선별 상태를 설명하지만, 공개 원천·좌표 검증 레코드는 아직 등록하지 않았습니다.")
            ],
            [
                "현재 검증된 홍익학당 철학·영상의 지리 레코드가 없어 지도 마커를 표시하지 않습니다.",
                "개인 주소, 촬영지·행사장, 채널·영상 설명에서 추정한 위치는 검증 기록으로 취급하지 않습니다.",
                "공개 가능한 원천 URL, 지리 범위, 검증 시각이 함께 등록된 뒤에만 비개인적 범위 마커를 추가합니다."
            ]));
    }
}
