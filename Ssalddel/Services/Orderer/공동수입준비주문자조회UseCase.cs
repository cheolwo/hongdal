using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공동수입준비주문자조회UseCase
{
    Task<공동수입준비주문자조회응답?> 조회Async(
        string 공동수입원장Id,
        string 자동집단Id,
        string 주문자UserId,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "로그인 주문자가 참여한 1.0 수요 집단에 연결된 공동수입 원장의 1.5 준비 자료만 조회합니다.",
    ContractType = typeof(I공동수입준비주문자조회UseCase),
    FlowOrder = 35,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "참여 여부와 원장 식별자 일치를 모두 확인하며 관리자 OS 실행·재시도, 외부 전송, 계약, 결제, 신고와 운송 동작을 제공하지 않습니다.")]
public sealed class 공동수입준비주문자조회UseCase : I공동수입준비주문자조회UseCase
{
    private readonly I공동구매자동집단화저장소 _자동집단저장소;
    private readonly I공동수입준비원장Service _준비원장Service;

    public 공동수입준비주문자조회UseCase(
        I공동구매자동집단화저장소 자동집단저장소,
        I공동수입준비원장Service 준비원장Service)
    {
        _자동집단저장소 = 자동집단저장소;
        _준비원장Service = 준비원장Service;
    }

    public async Task<공동수입준비주문자조회응답?> 조회Async(
        string 공동수입원장Id,
        string 자동집단Id,
        string 주문자UserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(공동수입원장Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(주문자UserId);

        var normalizedGroupId = 자동집단Id.Trim();
        var normalizedUserId = 주문자UserId.Trim();
        var group = await _자동집단저장소.집단조회Async(normalizedGroupId, cancellationToken);
        if (group is null
            || !group.수요목록.Any(demand => string.Equals(
                demand.주문자키,
                normalizedUserId,
                StringComparison.Ordinal)))
        {
            return null;
        }

        var readiness = await _준비원장Service.조회Async(normalizedGroupId, cancellationToken);
        if (readiness is null
            || !string.Equals(
                readiness.원장Id,
                공동수입원장Id.Trim(),
                StringComparison.Ordinal))
        {
            return null;
        }

        return 공동수입준비주문자Projection.생성(readiness);
    }
}
